using Mail.Contracts;
using System.Threading.Tasks;

namespace Mail.Services;

public interface IMailSender
{
    Task<bool> SendMail(Message message);
}