using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Utilities.Logging;
using Org.Apache.REEF.ParameterService.Multiverso.Bridge;

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
        public class ComponentAddresses : Name<ISet<string>> { }

        [NamedParameter("Configuration representing the number tables, rows and columns of parameters to be created")]
        public class TablesRowsColumns : Name<string> { }
    }

    internal class ParameterServiceClient : IParameterServiceClient
    {
        private readonly int[][] _tablesRowsColumns;
        private readonly CommunicationType _commType;
        private readonly SynchronizationType _syncType;

        private static readonly Logger LOGGER = Logger.GetLogger(typeof(ParameterServiceClient));

        [Inject]
        internal ParameterServiceClient([Parameter(typeof(ParameterClientConfig.CommunicationType))] string commType,
            [Parameter(typeof(ParameterClientConfig.SynchronizationType))] string syncType,
            [Parameter(typeof(ParameterClientConfig.ComponentAddresses))] IEnumerable<string> componentAddrPorts,
            [Parameter(typeof(ParameterClientConfig.TablesRowsColumns))] string  tablesRowsColumns,
            IParameterServer parameterServer,
            IParameterClient parameterClient)
        {
            if (!Enum.TryParse(commType, out _commType))
            {
                LOGGER.Log(Level.Warning,
                    "Unable to parse provided communication type. Using default of {0}",
                    CommunicationType.Reduce);
                _commType = CommunicationType.Reduce;
            }
            if (!Enum.TryParse(syncType, out _syncType))
            {
                LOGGER.Log(Level.Warning,
                    "Unable to parse provided synchronization type. Using default of {0}",
                    SynchronizationType.Average);
                _syncType = SynchronizationType.Average;
            }
            IDictionary<string, AddressPort> componentAddressPorts =
                componentAddrPorts.ToDictionary(idAddrPort => IdAddressPort.FromString(idAddrPort).ServerId,
                    idAddrPort => IdAddressPort.FromString(idAddrPort).AddrPort);

            parameterServer.UpdateOtherComponentAddresses(componentAddressPorts);
            parameterClient.UpdateOtherComponentAddresses(componentAddressPorts);

            _tablesRowsColumns = tablesRowsColumns.Split('|').Select(rowsStr => rowsStr.Split(':').Select(c=>Convert.ToInt32(c)).ToArray()).ToArray();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Get(int tableId, int[] pValue)
        {
            MultiversoWrapper.Get(tableId, pValue);
        }

        public void Get(int tableId, float[] pValue)
        {
            MultiversoWrapper.Get(tableId, pValue);
        }

        public void Get(int tableId, int rowId, int[] pValue)
        {
            MultiversoWrapper.Get(tableId, rowId, pValue);
        }

        public void Get(int tableId, int rowId, float[] pValue)
        {
            MultiversoWrapper.Get(tableId, rowId, pValue);
        }

        public void Add(int tableId, int[] pValue)
        {
            MultiversoWrapper.Add(tableId, pValue);
        }

        public void Add(int tableId, float[] pValue)
        {
            MultiversoWrapper.Add(tableId, pValue);
        }

        public void Add(int tableId, int rowId, int[] pDelta)
        {
            MultiversoWrapper.Add(tableId, rowId, pDelta);
        }

        public void Add(int tableId, int rowId, float[] pDelta)
        {
            MultiversoWrapper.Add(tableId, rowId, pDelta);
        }

        public void Barrier()
        {
            MultiversoWrapper.Barrier();
        }
    }
}