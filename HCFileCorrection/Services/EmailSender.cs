using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using OpenQA.Selenium;
using System.Net;
using System.Net.Mail;
using HCFileCorrection.Models;
using Microsoft.Extensions.Options;

namespace HCFileCorrection
{
    public class EmailSender
    {
        private readonly EmailSetting _emailSettings;
        private readonly IConfiguration _configuration;

        public EmailSender(IOptions<EmailSetting> emailSettings,IConfiguration configuration)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
        }

        public async Task SendEmail(string subject, string body)
        {
            string recipient = _configuration.GetValue<string>("Receipient");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("posfiledownloader@harpercollins.com", "posfiledownloader@harpercollins.com"));
            message.To.Add(new MailboxAddress("recipientname", recipient));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            try
            {
                await client.ConnectAsync("smtpscr.na.harpercollins.org", _emailSettings.SmtpPort);

                // await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as necessary
                throw new Exception("An error occurred while sending the email: " + ex.Message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
