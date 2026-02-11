using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using InnerG.Api.Services.Interfaces;
using InnerG.Api.Exceptions;

namespace InnerG.Api.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string subject, string html)
        {
            string host = _config["SMTP_HOST"]
                ?? throw new ConfigurationException("SMTP_HOST");

            string username = _config["SMTP_USERNAME"]
                ?? throw new ConfigurationException("SMTP_USERNAME");

            string password = _config["SMTP_PASSWORD"]
                ?? throw new ConfigurationException("SMTP_PASSWORD");

            string fromName = _config["SMTP_FROM_NAME"] ?? "Support";

            try
            {
                var message = new MailMessage
                {
                    Subject = subject,
                    Body = html,
                    IsBodyHtml = true,
                    From = new MailAddress(username, fromName)
                };

                message.To.Add(toEmail);

                using var smtp = new SmtpClient(host, 587)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(message);
            }
            catch (SmtpException)
            {
                throw new ExternalServiceException("Failed to send confirmation email");
            }
        }

    }
}