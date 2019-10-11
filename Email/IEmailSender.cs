using System.Threading.Tasks;

namespace YounesCo_Backend.Email
{
    public interface IEmailSender
    {
        //Sends Email with the given information 
        Task<SendEmailResponse> SendEmailAsync(string UserEmail, string emailSubject, string message);
    }
}
