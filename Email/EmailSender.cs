using SendGrid;
using SendGrid.Helpers.Mail;

namespace IdentityTest2.Email
{
    public class EmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string Useremail, string emailSubject, string msg) 
        {
            var client = new SendGridClient(_config["SendGrid:key"]);
            var message = new SendGridMessage
            {
                From = new EmailAddress("dohung640@outlook.com", _config["SendGrid:User"]),
                Subject = emailSubject,
                PlainTextContent = msg,
                HtmlContent = msg
            };
            message.AddTo(new EmailAddress(Useremail));
            message.SetClickTracking(false, false);

            await client.SendEmailAsync(message);
        }
    }
}
