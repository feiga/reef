// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Org.Apache.REEF.Common.Io;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Utilities.Logging;
using Org.Apache.REEF.Wake.Remote;
using Org.Apache.REEF.ParameterService.Multiverso.Bridge;

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
