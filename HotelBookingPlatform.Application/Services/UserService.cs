using Microsoft.AspNetCore.Identity;

namespace HotelBookingPlatform.Application.Services;
public class UserService : IUserService
{
    private readonly UserManager<LocalUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    public UserService(UserManager<LocalUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    }
    public async Task<AuthModel> RegisterAsync(RegisterModel model)
    {
        var validRoles = new List<string> { "Admin", "User", "Staff" };
        if (!validRoles.Contains(model.Role))
        {
            throw new BadRequestException("Invalid role. Allowed roles are 'Admin' or 'User' or 'Staff'.");
        }

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
            throw new BadRequestException("Email is already registered!");

        var user = new LocalUser
        {
            UserName = model.Email.Split('@')[0],
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(error => error.Description));
            throw new BadRequestException(errors);
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }

        await _userManager.AddToRoleAsync(user, model.Role);

        var jwtSecurityToken = await _tokenService.CreateJwtToken(user);

        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokens?.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        return new AuthModel
        {
            Email = user.Email,
            ExpiresOn = jwtSecurityToken.ValidTo,
            IsAuthenticated = true,
            Roles = new List<string> { model.Role },
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            Username = user.UserName,
            Message = "Registration successful.",
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpiresOn
        };
    }

    public async Task<AuthModel> LoginAsync(LoginModel model)
    {
        var authModel = new AuthModel();

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            throw new UnauthorizedAccessException("Email or Password is incorrect!");

        var jwtSecurityToken = await _tokenService.CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        authModel.IsAuthenticated = true;
        authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        authModel.Email = user.Email;
        authModel.Username = user.UserName;
        authModel.ExpiresOn = jwtSecurityToken.ValidTo;
        authModel.Roles = rolesList.ToList();
        authModel.Message = "Login successful.";

        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            authModel.RefreshToken = activeRefreshToken.Token;
            authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var refreshToken = _tokenService.GenerateRefreshToken();
            authModel.RefreshToken = refreshToken.Token;
            authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }

        return authModel;
    }
}
