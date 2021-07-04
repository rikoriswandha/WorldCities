using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using PostmarkDotNet;

namespace WorldCities.Services
{
    public class PostmarkEmailSender : IEmailSender
    {
        public PostmarkEmailSender(
            IOptions<PostmarkEmailSenderOptions> options
        )
        {
            this.Options = options.Value;
        }

        public PostmarkEmailSenderOptions Options { get; set; }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new System.NotImplementedException();
        }

        private async Task<PostmarkResponse> Execute(
            string apiKey,
            string subject,
            string message,
            string email
        )
        {
            var client = new PostmarkClient(apiKey);
            var msg = new PostmarkMessage()
            {
                From = Options.Sender_Email,
                To = email,
                TrackOpens = false,
                Subject = subject,
                HtmlBody = message
            };

            return await client.SendMessageAsync(msg);
        }
    }
}