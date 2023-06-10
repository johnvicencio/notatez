using System;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notatez.Models.Services;

public class SendGridEmailService
{
    private readonly string _apiKey;

    public SendGridEmailService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task SendEmailAsync(string recipientEmail, string recipientName, string subject, string message)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress("jvicencio+notatez@johnvicencio.com", "Notatez TEST");
        var to = new EmailAddress(recipientEmail, recipientName);
        var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, message, null);

        var response = await client.SendEmailAsync(emailMessage);

        if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
        {
            // Handle the error or log it
            throw new Exception($"Failed to send email. Status code: {response.StatusCode}");
        }
    }
}
