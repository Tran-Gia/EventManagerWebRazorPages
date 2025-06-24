using MimeKit;
using MimeKit.Utils;

namespace EventManagerWebRazorPage.Services.Other
{
    public static class EmailSender
    {
        private static readonly string _emailFrom = GlobalConfig.EmailFrom;
        private static readonly string _emailPassword = GlobalConfig.EmailFromPassword;
        public static async Task SendNotificationEmail(List<string> emails, string emailTitle, string emailMessage, string? imagePath = "")
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Organizer", _emailFrom));
                message.Body = message.StandardEmailBuilder(emails, emailTitle, emailMessage, imagePath).ToMessageBody();
                await message.SendEmail();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return;
        }


        private static BodyBuilder StandardEmailBuilder(this MimeMessage message, List<string> emails, string emailTitle, string emailMessage)
        {
            var builder = new BodyBuilder();
            foreach (string email in emails)
            {
                message.To.Add(new MailboxAddress("Users", email));
            }
            message.Subject = emailTitle;
            builder.TextBody = emailMessage;
            return builder;
        }
        private static BodyBuilder StandardEmailBuilder(this MimeMessage message, List<string> emails, string emailTitle, string emailMessage, string? imagePath)
        {
            var builder = message.StandardEmailBuilder(emails, emailTitle, emailMessage);
            if(!String.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var image = builder.LinkedResources.Add(imagePath);
                image.ContentId = MimeUtils.GenerateMessageId();
                builder.HtmlBody = $"{builder.TextBody}<br/><img src='cid:{image.ContentId}' style='max-height:200px;max-width:200px'>";
                builder.Attachments.Add(imagePath);
            }
            return builder;
        }

        private static async Task SendEmail(this MimeMessage message)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(GlobalConfig.EmailHost, 587, false);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(_emailFrom, _emailPassword);

                await client.SendAsync(message);
                client.Disconnect(true);
            }
            return;
        }
    }
}
