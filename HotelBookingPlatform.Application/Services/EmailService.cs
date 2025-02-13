using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using HotelBookingPlatform.Application.Services;

namespace HotelBookingPlatform.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILog _log;

    public EmailService(IOptions<EmailSettings> emailSettings, ILog log)
    {
        _emailSettings = emailSettings.Value;
        _log = log;
    }

    public async Task SendConfirmationEmailAsync(BookingConfirmation confirmation)
    {
        if (confirmation is null)
        {
            _log.Log("Booking confirmation object is null.", "warning");
            return;
        }

        var email = CreateMimeMessage(confirmation);

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _log.Log($"Confirmation email sent to {confirmation.UserEmail}.", "info");
        }
        catch (Exception ex)
        {
            _log.Log($"Error while sending email: {ex.Message}", "error");
        }
    }

    private MimeMessage CreateMimeMessage(BookingConfirmation confirmation)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Hotel Booking Platform", _emailSettings.FromAddress));
        email.To.Add(MailboxAddress.Parse(confirmation.UserEmail));
        email.Subject = "Booking Confirmation";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = GenerateEmailBody(confirmation)
        };

        email.Body = bodyBuilder.ToMessageBody();
        return email;
    }

    private string GenerateEmailBody(BookingConfirmation confirmation)
    {
        return $@"
            <html>
            <body>
                <h2>Booking Confirmation</h2>
                <p><strong>Confirmation Number:</strong> {confirmation.ConfirmationNumber}</p>
                <p><strong>Hotel Name:</strong> {confirmation.HotelName}</p>
                <p><strong>Hotel Address:</strong> {confirmation.HotelAddress}</p>
                <p><strong>Room Type:</strong> {confirmation.RoomType}</p>
                <p><strong>Check-In Date:</strong> {confirmation.CheckInDate:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Check-Out Date:</strong> {confirmation.CheckOutDate:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Total Price:</strong> {confirmation.TotalPrice:C}</p>
                <p><strong>Discount Percentage:</strong> {confirmation.Percentage}%</p>
                <p><strong>Price After Discount:</strong> {confirmation.AfterDiscountedPrice:C}</p>
                <p><strong>User Email:</strong> {confirmation.UserEmail}</p>
                <p>Thank you for booking with us!</p>
            </body>
            </html>";
    }
}
