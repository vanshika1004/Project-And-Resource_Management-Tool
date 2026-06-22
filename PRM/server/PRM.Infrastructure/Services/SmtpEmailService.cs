using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PRM.Application.Interfaces;
using PRM.Core.Interfaces;

namespace PRM.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly ISystemConfigRepository _configRepo;

    public SmtpEmailService(ISystemConfigRepository configRepo)
    {
        _configRepo = configRepo;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = await _configRepo.GetValueAsync("SmtpHost");
        var portStr = await _configRepo.GetValueAsync("SmtpPort");
        var user = await _configRepo.GetValueAsync("SmtpUser");
        var password = await _configRepo.GetValueAsync("SmtpPassword");
        var fromEmail = await _configRepo.GetValueAsync("FromEmail");

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr) || string.IsNullOrWhiteSpace(fromEmail))
        {
            // Fallback to console logging if SMTP is not configured
            Console.WriteLine("=========================================");
            Console.WriteLine($"[EMAIL MOCK] To: {to}");
            Console.WriteLine($"[EMAIL MOCK] Subject: {subject}");
            Console.WriteLine($"[EMAIL MOCK] Body:\n{body}");
            Console.WriteLine("=========================================");
            return;
        }

        if (!int.TryParse(portStr, out int port)) port = 587;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("PRM System", fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, SecureSocketOptions.Auto);

            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
            {
                await client.AuthenticateAsync(user, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] Failed to send email to {to}: {ex.Message}");
            throw; // Rethrow to let caller handle if necessary, or just log
        }
    }
}
