using API_dormitory.Models.Users;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;
using DotNetEnv;

public class EmailService
{
    private readonly TransactionalEmailsApi _emailApi;

    public EmailService()
    {
        // Đọc tệp .env
        Env.Load();

        // Lấy API key từ .env
        string apiKey = Env.GetString("SENDINBLUE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API key for Sendinblue is not set.");
        }
        Configuration.Default.AddApiKey("api-key", apiKey);
        _emailApi = new TransactionalEmailsApi();
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        var sender = new SendSmtpEmailSender(name: "KTX-QNU", email: "thuhue12tn1@gmail.com");

        var to = new List<SendSmtpEmailTo>
        {
            new SendSmtpEmailTo(toEmail, toName)
        };

        var email = new SendSmtpEmail
        {
            Sender = sender,
            To = to,
            Subject = subject,
            HtmlContent = htmlContent
        };

        var response = await _emailApi.SendTransacEmailAsync(email);
        Console.WriteLine("Email sent: " + response.MessageId);
    }

    internal async Task SendEmailAsync(AccountModels account, string v1, string v2, string v3)
    {
        throw new NotImplementedException();
    }
}
