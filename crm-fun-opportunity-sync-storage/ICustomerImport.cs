using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.Service
{
    public interface ICustomerImport
    {
        Task HandleMessage(string queueMsg, System.Collections.Generic.IDictionary<string, object> userProperties);
    }
}
