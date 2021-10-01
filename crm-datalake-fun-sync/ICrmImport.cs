using System.Threading.Tasks;

namespace Crm.Service
{
    public interface ICrmImport
    {
        Task HandleMessage(string queueMsg);
    }
}
