using Core.Helpers;
using Core.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Core.Repository
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _config;
        private readonly Properties.MailSettings _mailSettings;

        public MailService(IConfiguration configuration, IOptions<Properties.MailSettings> mailSettings)
        {
            _config = configuration;
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }
            // var password = Encoding.UTF8.GetString(Convert.FromBase64String(_mailSettings.Password));
            var password = _mailSettings.Password;

            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
            
        }
        // public async Task SendEmailAsync(string toEmail, string subject, string recap)
        // {
        //    var keyMail = System.Convert.FromBase64String(_config["SendMailAPIKey"]);
        //    string apiKey = System.Text.Encoding.UTF8.GetString(keyMail);
        //    var client = new SendGridClient(apiKey);
        //    var from = new EmailAddress("9277.dev@gmail.com","Tooth Fairy");
        //    var to = new EmailAddress(toEmail, "Re:Test Mail");
        //    var msg = MailHelper.CreateSingleEmail(from, to, subject, recap, recap);
        //    var response = await client.SendEmailAsync(msg);
        // }
    }
}