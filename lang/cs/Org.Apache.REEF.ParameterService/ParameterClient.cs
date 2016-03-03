using System.Collections.Generic;
using System.Linq;
using System.Net;
using Org.Apache.REEF.Common.Io;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Wake.Remote;

namespace Org.Apache.REEF.ParameterService
{
    class ParameterClient : IParameterClient
    {
        private readonly string _clientId;
        private readonly INameClient _nameClient;

        [Inject]
        internal ParameterClient([Parameter(typeof(ParameterClientId))] string clientId,
            INameClient nameClient,
            ITcpPortProvider tcpPortProvider)
        {
            _clientId = clientId;
            _nameClient = nameClient;
            var ipEndPoint = StartClient(tcpPortProvider);
            _nameClient.Register(clientId, ipEndPoint);
        }

        private static IPEndPoint StartClient(ITcpPortProvider tcpPortProvider)
        {
            //Instead of this start the actual parameter server
            //Get the ip & port it is listerning
            //Register that ip & port
            return new IPEndPoint(IPAddress.Loopback, tcpPortProvider.First());
        }

        public void UpdateOtherComponentAddresses(IDictionary<string, AddressPort> addressPorts)
        {
            
        }

        public void Dispose()
        {
            //Free resources
            _nameClient.Unregister(_clientId);
        }
    }
}
