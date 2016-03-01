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
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Utilities.Logging;

namespace Org.Apache.REEF.ParameterService
{
    public enum CommunicationType
    {
        Reduce, P2P
    }

    public enum SynchronizationType
    {
        Average, Async
    }

    internal class AddressPort
    {
        public IPAddress Address { get; private set; }

        public int Port { get; private set; }

        public AddressPort(IPAddress address, int port)
        {
            Address = address;
            Port = port;
        }

        public AddressPort(IPEndPoint endpoint)
        {
            Address = endpoint.Address;
            Port = endpoint.Port;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", Address, Port);
        }

        public static AddressPort FromString(string serAddrPort)
        {
            var tokens = serAddrPort.Split(new char[] { '|' }, 2);
            return new AddressPort(IPAddress.Parse(tokens[0]), Convert.ToInt32(tokens[1]));
        }
    }

    internal class IdAddressPort
    {
        public string ServerId { get; private set; }
        public AddressPort AddrPort { get; private set; }

        public IdAddressPort(string serverId, AddressPort addrPort)
        {
            ServerId = serverId;
            AddrPort = addrPort;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}|{1}", ServerId, AddrPort);
        }

        public static IdAddressPort FromString(string serIdAddrPort)
        {
            var tokens = serIdAddrPort.Split(new char[] { '|' }, 2);
            return new IdAddressPort(tokens[0], AddressPort.FromString(tokens[1]));
        }
    }

    public class ParameterClientConfig
    {
        [NamedParameter("Communication Type")]
        public class CommunicationType : Name<string> { }

        [NamedParameter("Synchronization Type")]
        public class SynchronizationType : Name<string> { }

        [NamedParameter("The set of <ip:port> where the servers are listening")]
        public class ServerAddresses : Name<ISet<string>> { }

        [NamedParameter("The number of tables of parameters to be configured on the parameter server")]
        public class NumberOfTables : Name<int> { }

        [NamedParameter("The number of rows of parameters per table to be configured on the parameter server")]
        public class NumberOfRows : Name<int> { }

        [NamedParameter("The number of parameters for each row of every table to be configured on the parameter server")]
        public class NumberOfColumns : Name<int> { }
    }

    internal class ParameterClient : IParameterClient
    {
        private readonly int _numberOfTables;
        private readonly int _numberOfRows;
        private readonly int _numberOfColumns;
        private readonly CommunicationType _commType;
        private readonly SynchronizationType _syncType;
        private IDictionary<string,AddressPort> _serverAddressPorts;

        private static readonly Logger LOGGER = Logger.GetLogger(typeof(ParameterClient));

        [Inject]
        internal ParameterClient([Parameter(typeof(ParameterClientConfig.CommunicationType))] string commType,
            [Parameter(typeof(ParameterClientConfig.SynchronizationType))] string syncType,
            [Parameter(typeof(ParameterClientConfig.ServerAddresses))] IEnumerable<string> serverAddrPorts,
            [Parameter(typeof(ParameterClientConfig.NumberOfTables))] int numberOfTables,
            [Parameter(typeof(ParameterClientConfig.NumberOfRows))] int numberOfRows,
            [Parameter(typeof(ParameterClientConfig.NumberOfColumns))] int numberOfColumns)
        {
            if (!Enum.TryParse(commType, out _commType))
            {
                LOGGER.Log(Level.Warning, "Unable to parse provided communication type. Using default of {0}", CommunicationType.Reduce);
                _commType = CommunicationType.Reduce;
            }
            if (!Enum.TryParse(syncType, out _syncType))
            {
                LOGGER.Log(Level.Warning, "Unable to parse provided synchronization type. Using default of {0}", SynchronizationType.Average);
                _syncType = SynchronizationType.Average;
            }
            _serverAddressPorts =
                serverAddrPorts.ToDictionary(idAddrPort => IdAddressPort.FromString(idAddrPort).ServerId,
                    idAddrPort => IdAddressPort.FromString(idAddrPort).AddrPort);

            _numberOfTables = numberOfTables;
            _numberOfRows = numberOfRows;
            _numberOfColumns = numberOfColumns;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Add(int tableId, long rowId, float[] pDelta, float coeff)
        {
            throw new NotImplementedException();
        }

        public void Add(int tableId, long rowId, int[] pDelta, float coeff)
        {
            throw new NotImplementedException();
        }

        public void AsyncGet(int tableId, long[] rows, float[][] pValues)
        {
            throw new NotImplementedException();
        }

        public void AsyncGet(int tableId, long[] rows, int[][] pValues)
        {
            throw new NotImplementedException();
        }

        public void Barrier()
        {
            throw new NotImplementedException();
        }

        public long BatchLoad(int tableId, long[] rows)
        {
            throw new NotImplementedException();
        }

        public void Clock()
        {
            throw new NotImplementedException();
        }

        public void Get(int tableId, long rowId, float[] pValue)
        {
            throw new NotImplementedException();
        }

        public void Get(int tableId, long rowId, int[] pValue)
        {
            throw new NotImplementedException();
        }

        public void Set(int tableId, long rowId, float[] pValue)
        {
            throw new NotImplementedException();
        }

        public void Set(int tableId, long rowId, int[] pValue)
        {
            throw new NotImplementedException();
        }
    }
}