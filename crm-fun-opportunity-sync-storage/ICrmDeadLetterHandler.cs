using System.Threading.Tasks;

namespace Crm.Service
{
    public interface ICrmDeadLetterHandler
    {
        Task HandleDeadletter();
    }
}
