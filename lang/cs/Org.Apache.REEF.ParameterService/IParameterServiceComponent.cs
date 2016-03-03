using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Apache.REEF.ParameterService
{
    interface IParameterServiceComponent : IDisposable
    {
        void UpdateOtherComponentAddresses(IDictionary<string, AddressPort> addressPorts);
    }
}
