using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelBookingPlatform.Infrastructure;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/auth")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IResponseHandler _responseHandler;
    private readonly ILog _logger;
    private readonly UserManager<LocalUser> _userManager;

    public UserController(
        IUserService userService,
        ITokenService tokenService,
        IResponseHandler responseHandler,
        ILog logger,
        UserManager<LocalUser> userManager)
    {
        _userService = userService;
        _tokenService = tokenService;
        _responseHandler = responseHandler;
        _logger = logger;
        _userManager = userManager;
    }

    // ✅ helper robuste pour lire l’Id du user depuis différents tokens
    private string? GetUserId()
    {
        // standard Identity
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(id)) return id;

        // jwt standard
        id = User.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(id)) return id;

        // parfois nameid existe directement
        id = User.FindFirstValue("nameid");
        if (!string.IsNullOrWhiteSpace(id)) return id;

        // fallback
        id = User.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(id)) return id;

        return null;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "UserId claim not found in token." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        var roles = await _userManager.GetRolesAsync(user);

        return _responseHandler.Success(new
        {
            user.Id,
            user.Email,
            user.UserName,
            Roles = roles
        }, "Current user profile.");
    }

    [Authorize(Policy = "AdminPolicy")]
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Create New Account",
        Description = "This endpoint allows a user to create a new account by providing the necessary details such as username, password, and email.")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
    {
        var result = await _userService.RegisterAsync(model);

        if (!result.IsAuthenticated)
        {
            _logger.Log($"Registration failed: {result.Message}", "warning");
            return _responseHandler.BadRequest(result.Message);
        }

        _tokenService.SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
        return _responseHandler.Success(result, "User registered successfully.");
    }

    [HttpPost("login")]
    [SwaggerOperation(Summary = "Authenticate User and Generate Token",
        Description = "Authenticates the user with the provided email and password. If the credentials are valid, returns a token and user information.")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
    {
        var result = await _userService.LoginAsync(model);

        if (!result.IsAuthenticated)
        {
            _logger.Log($"Login failed: {result.Message}", "warning");
            return _responseHandler.BadRequest(result.Message);
        }

        if (!string.IsNullOrEmpty(result.RefreshToken))
            _tokenService.SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return _responseHandler.Success(result, "User logged in successfully.");
    }
}
