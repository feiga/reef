﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using Org.Apache.REEF.Common.Io;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Wake.Remote;

namespace Org.Apache.REEF.ParameterService
{
    internal class ParameterServer : IParameterServer
    {
        private readonly string _serverId;
        private readonly INameClient _nameClient;

        [Inject]
        internal ParameterServer([Parameter(typeof(ParameterServerId))] string serverId,
            INameClient nameClient,
            ITcpPortProvider tcpPortProvider)
        {
            _serverId = serverId;
            _nameClient = nameClient;
            var ipEndPoint = StartServer(tcpPortProvider);
            _nameClient.Register(serverId, ipEndPoint);
        }

        private static IPEndPoint StartServer(ITcpPortProvider tcpPortProvider)
        {
            //Instead of this start the actual parameter server
            //Get the ip & port it is listerning
            //Register that ip & port
            return new IPEndPoint(IPAddress.Loopback, tcpPortProvider.First());
        }

        public void UpdateOtherComponentAddresses(IDictionary<string, AddressPort> addressPorts)
        {
            //Update the ip & ports of all involved components
            //Be sure to ignore your own! :)
        }

        public void Dispose()
        {
            //Free resources
            _nameClient.Unregister(_serverId);
        }
    }
}
