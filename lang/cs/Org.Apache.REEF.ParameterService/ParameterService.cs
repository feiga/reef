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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Org.Apache.REEF.Common.Io;
using Org.Apache.REEF.Common.Services;
using Org.Apache.REEF.Common.Tasks;
using Org.Apache.REEF.Driver.Context;
using Org.Apache.REEF.Driver.Evaluator;
using Org.Apache.REEF.Network.Naming;
using Org.Apache.REEF.Network.Utilities;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Tang.Exceptions;
using Org.Apache.REEF.Tang.Formats;
using Org.Apache.REEF.Tang.Implementations.Configuration;
using Org.Apache.REEF.Tang.Implementations.Tang;
using Org.Apache.REEF.Tang.Interface;
using Org.Apache.REEF.Tang.Util;
using Org.Apache.REEF.Utilities.Logging;
using Org.Apache.REEF.Wake.Remote.Parameters;
using ContextConfiguration = Org.Apache.REEF.Common.Context.ContextConfiguration;

namespace Org.Apache.REEF.ParameterService
{
    public enum ElementType
    {
        Single, Double
    }

    public sealed class ParameterServiceBuilderConfiguration : ConfigurationModuleBuilder
    {
        /// <summary>
        ///  TCP Port range start
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:Do not declare read only mutable reference types",
            Justification = "not applicable")] public static readonly RequiredParameter<int> StartingPort =
                new RequiredParameter<int>();

        /// <summary>
        ///  TCP Port range count
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:Do not declare read only mutable reference types",
            Justification = "not applicable")] public static readonly RequiredParameter<int> PortRange =
                new RequiredParameter<int>();

        public static ConfigurationModule ConfigurationModule
        {
            get
            {
                return new TaskConfiguration().BindNamedParameter(GenericType<TcpPortRangeStart>.Class, StartingPort)
                    .BindNamedParameter(GenericType<TcpPortRangeCount>.Class, PortRange).Build();
            }
        }
    }

    public class ParameterServiceBuilder
    {
        private const int ParameterServiceCores = 0;

        private readonly int _startingPort;
        private readonly int _portRange;
        private readonly INameServer _nameServer;
        private readonly INameClient _nameClient;
        private readonly IEvaluatorRequestor _evaluatorRequestor;

        private CommunicationType _communication = CommunicationType.Reduce;
        private SynchronizationType _synchronization = SynchronizationType.Average;
        private ElementType _element = ElementType.Single;
        private int _taskCores = 2;

        private int _numTasks;
        private int _taskMemMB;
        private int[][] _tablesRowsColumns;

        [Inject]
        private ParameterServiceBuilder([Parameter(typeof(TcpPortRangeStart))] int startingPort,
            [Parameter(typeof(TcpPortRangeCount))] int portRange,
            INameServer nameServer,
            INameClient nameClient,
            IEvaluatorRequestor evaluatorRequestor)
        {
            _startingPort = startingPort;
            _portRange = portRange;
            _nameServer = nameServer;
            _nameClient = nameClient;
            _evaluatorRequestor = evaluatorRequestor;
        }

        public ParameterServiceBuilder SetCommunicationType(CommunicationType communication)
        {
            _communication = communication;
            return this;
        }

        public ParameterServiceBuilder SetSynchronizationType(SynchronizationType synchronization)
        {
            _synchronization = synchronization;
            return this;
        }

        /// <summary>
        /// Number of tasks to run, possibly equal to the number of file partitions. 
        /// Will create as many evaluators as the number of tasks specified
        /// </summary>
        /// <param name="numTasks"> Number of tasks to run </param>
        /// <returns></returns>
        public ParameterServiceBuilder SetNumberOfTasks(int numTasks)
        {
            _numTasks = numTasks;
            return this;
        }

        /// <summary>
        /// Memory requirements for loading data into memory
        /// </summary>
        /// <param name="taskMemMB">required memory in MB</param>
        /// <returns></returns>
        public ParameterServiceBuilder SetTaskMemoryMB(int taskMemMB)
        {
            _taskMemMB = taskMemMB;
            return this;
        }

        /// <summary>
        /// Core requirements for users tasks
        /// </summary>
        /// <param name="taskCores">number of cores</param>
        /// <returns></returns>
        public ParameterServiceBuilder SetTaskCores(int taskCores)
        {
            _taskCores = taskCores;
            return this;
        }

        /// <summary>
        /// The number of tables of parameters, number of rows per table
        /// and the number of columns in each row of a table that should
        /// be created on the Parameter Server.
        /// </summary>
        /// <param name="tablesRowsColumns"> The length of the 2d array
        /// represents the #tables. The length of each 1d array represents the #rows
        /// for the table corresponding to the index of the 1d array in the 2d array.
        /// Element (i,j) represents the #columns for row j of table i</param>
        /// <returns></returns>
        public ParameterServiceBuilder SetTablesRowsColumns(int[][] tablesRowsColumns)
        {
            if (tablesRowsColumns.Length == 0)
                throw new IllegalStateException("Required number of tables can't be zero");
            var numTables = tablesRowsColumns.Length;
            var emptyTables = string.Join(",",
                Enumerable.Range(0, numTables).Where(i => tablesRowsColumns[i].Length == 0));
            if(!string.IsNullOrEmpty(emptyTables))
                throw new IllegalStateException(string.Format("Tables: {0} are empty", emptyTables));
            var emptyRows = string.Join(", ",
                Enumerable.Range(0, numTables)
                    .SelectMany(
                        i =>
                            Enumerable.Range(0, tablesRowsColumns[i].Length)
                                .Where(j => tablesRowsColumns[i][j] == 0)
                                .Select(j => string.Format("({0},{1})", i, j))));
            if(!string.IsNullOrEmpty(emptyRows))
                throw new IllegalStateException(string.Format("The following (Table,Row) pairs are empty: {0}", emptyRows));
            _tablesRowsColumns = tablesRowsColumns;
            return this;
        }

        public ParameterServiceBuilder SetElementType(ElementType element)
        {
            _element = element;
            return this;
        }

        public IParameterService Build()
        {
            if (!RequiredParametersSet())
                throw new IllegalStateException(
                    "Number of Tasks, Number of Tables, Number of Rows, Number of Columns & Task Memory MB are required parameters");
            var totalMemoryPerTask = _taskMemMB + GetMemoryInMBForParameterService();
            var totalCoresPerTask = _taskCores + GetCoresForParameterService();
            var evaluatorRequest = _evaluatorRequestor.NewBuilder()
                .SetCores(totalCoresPerTask)
                .SetMegabytes(totalMemoryPerTask)
                .SetNumber(_numTasks).Build();
            var tcpPortProviderConfig = TangFactory.GetTang().NewConfigurationBuilder()
                .BindNamedParameter<TcpPortRangeStart, int>(GenericType<TcpPortRangeStart>.Class,
                    _startingPort.ToString(CultureInfo.InvariantCulture))
                .BindNamedParameter<TcpPortRangeCount, int>(GenericType<TcpPortRangeCount>.Class,
                    _portRange.ToString(CultureInfo.InvariantCulture))
                .Build();

            var namingConfig = TangFactory.GetTang().NewConfigurationBuilder()
                .BindNamedParameter<NamingConfigurationOptions.NameServerAddress, string>(
                    GenericType<NamingConfigurationOptions.NameServerAddress>.Class,
                    _nameServer.LocalEndpoint.Address.ToString())
                .BindNamedParameter<NamingConfigurationOptions.NameServerPort, int>(
                    GenericType<NamingConfigurationOptions.NameServerPort>.Class,
                    _nameServer.LocalEndpoint.Port.ToString(CultureInfo.InvariantCulture))
                .BindImplementation(GenericType<INameClient>.Class,
                    GenericType<NameClient>.Class).Build();
            return new ParameterService(tcpPortProviderConfig, 
                namingConfig,
                _numTasks,
                _tablesRowsColumns,
                _nameClient,
                _communication,
                _synchronization,
                _evaluatorRequestor,
                evaluatorRequest);
        }

        /// <summary>
        /// Another simple heuristic for computing cores requirement for parameter service
        /// Currently assuming it to be zero
        /// </summary>
        /// <returns>The number of cores required for running the Parameter Service for the given
        /// configuration of parameters</returns>
        private static int GetCoresForParameterService()
        {
            return ParameterServiceCores;
        }

        /// <summary>
        /// Currently a simple heuristic to compute the memory requirements for the parameter service
        /// Client & Server - assuming double caching - hence the multiplier 2
        /// </summary>
        /// <returns>The amount of memory in MB required for running the Parameter Service for the
        /// given configuration of parameters</returns>
        private int GetMemoryInMBForParameterService()
        {
            var perElementSize = _element == ElementType.Single ? sizeof (float) : sizeof (double);
            var numberOfParameters = _tablesRowsColumns.Select(trc=>trc.Sum()).Sum();
            return (int) Math.Ceiling(numberOfParameters*perElementSize*2/(double) (_numTasks*1 << 20));
        }

        private bool RequiredParametersSet()
        {
            return _numTasks != 0 && _tablesRowsColumns!=null && _taskMemMB != 0;
        }
    }

    [NamedParameter("The number of tasks to be launched")]
    public class NumberOfTasks : Name<int> { }
    class ParameterService : IParameterService
    {
        private readonly int _numTasks;
        private readonly int[][] _tablesRowsColumns;
        private readonly INameClient _nameClient;
        private const string TaskContextName = "TaskContext";
        private const string ServerIdPrefix = "ParameterServer";
        private const string ClientIdPrefix = "ParameterClient";

        private static readonly Logger LOGGER = Logger.GetLogger(typeof(ParameterService));

        private int _contextIds;
        private int _componentIds;
        private readonly object _lock = new object();
        private readonly ConcurrentQueue<Tuple<IConfiguration, IActiveContext>> _taskTuples = new ConcurrentQueue<Tuple<IConfiguration, IActiveContext>>();
        private readonly CommunicationType _communication;
        private readonly SynchronizationType _synchronization;
        private readonly IEvaluatorRequestor _evaluatorRequestor;
        private readonly IEvaluatorRequest _evaluatorRequest;
        private readonly IConfiguration _tcpPortProviderConfig;
        private readonly IConfiguration _namingConfig;

        internal ParameterService(IConfiguration tcpPortProviderConfig,
            IConfiguration namingConfig,
            int numTasks,
            int[][] tablesRowsColumns,
            INameClient nameClient,
            CommunicationType communication,
            SynchronizationType synchronization,
            IEvaluatorRequestor evaluatorRequestor,
            IEvaluatorRequest evaluatorRequest)
        {
            _tcpPortProviderConfig = tcpPortProviderConfig;
            _namingConfig = namingConfig;
            _numTasks = numTasks;
            _tablesRowsColumns = tablesRowsColumns;
            _nameClient = nameClient;
            _communication = communication;
            _synchronization = synchronization;
            _evaluatorRequestor = evaluatorRequestor;
            _evaluatorRequest = evaluatorRequest;
        }

        public void RequestEvaluators()
        {
            _evaluatorRequestor.Submit(_evaluatorRequest);
        }

        /// <summary>
        /// Queues the task into the TaskStarter.
        /// 
        /// Once the correct number of tasks have been queued, the final Configuration
        /// will be generated and run on the given Active Context.
        /// </summary>
        /// <param name="partialTaskConfig">The partial task configuration containing Task
        /// identifier and Task class</param>
        /// <param name="activeContext">The Active Context to run the Task on</param>
        public void QueueTask(IConfiguration partialTaskConfig, IActiveContext activeContext)
        {
            _taskTuples.Enqueue(new Tuple<IConfiguration, IActiveContext>(partialTaskConfig, activeContext));
            if(_taskTuples.Count!=_numTasks)
                return;

            LOGGER.Log(Level.Verbose, "Expected number of tasks have been queued. So launching the Tasks");
            var clientConf = GetParameterServiceClientConfiguration();
            Parallel.ForEach(_taskTuples,
                tuple =>
                {
                    var userConf = tuple.Item1;
                    var actContext = tuple.Item2;
                    actContext.SubmitTask(Configurations.Merge(userConf, clientConf));
                });
        }

        public IConfiguration GetLocalContextConfiguration()
        {
            var contextNum = Interlocked.Increment(ref _contextIds);
            return
                ContextConfiguration.ConfigurationModule.Set(ContextConfiguration.Identifier,
                    string.Format(CultureInfo.InvariantCulture, "{0}-{1}", TaskContextName, contextNum)).Build();
        }

        public IConfiguration GetParameterServiceConfiguration()
        {
            //Check if this works
            var serviceConfiguration =
                ServiceConfiguration.ConfigurationModule
                    .Set(ServiceConfiguration.Services, GenericType<ParameterServer>.Class)
                    .Set(ServiceConfiguration.Services, GenericType<ParameterClient>.Class)
                    .Build();
            var componentId = Interlocked.Increment(ref _componentIds);
            var serverIdConfig =
                TangFactory.GetTang().NewConfigurationBuilder()
                    .BindNamedParameter<ParameterServerId, string>(GenericType<ParameterServerId>.Class,
                        GetServerId(componentId)).Build();

            var clientIdConfig =
                TangFactory.GetTang().NewConfigurationBuilder()
                    .BindNamedParameter<ParameterClientId, string>(GenericType<ParameterClientId>.Class,
                        GetClientId(componentId)).Build();

            return Configurations.Merge(serviceConfiguration, serverIdConfig, clientIdConfig, _tcpPortProviderConfig, _namingConfig);
        }

        private static string GetServerId(int componentId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", ServerIdPrefix, componentId);
        }

        private static string GetClientId(int componentId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", ClientIdPrefix, componentId);
        }

        private IConfiguration GetParameterServiceClientConfiguration()
        {
            var serverIds = Enumerable.Range(0, _numTasks).Select(GetServerId);
            var clientIds = Enumerable.Range(0, _numTasks).Select(GetClientId);
            var componentIds = serverIds.Concat(clientIds).ToList();
            var nameAssignments = _nameClient.Lookup(componentIds);
            if (nameAssignments.Any(na => na.Endpoint == null))
            {
                LOGGER.Log(Level.Error,
                    "Parameter Service Components {0} have not yet registered. It is an error to call this function before all components have registered.",
                    string.Join(",", nameAssignments.Where(na => na.Endpoint == null).Select(na => na.Identifier)));
                throw new IllegalStateException("Some parameter service components have not yet registered.");
            }
            var configurationBuilder =
                TangFactory.GetTang()
                    .NewConfigurationBuilder()
                    .BindNamedParameter<ParameterClientConfig.CommunicationType, string>(
                        GenericType<ParameterClientConfig.CommunicationType>.Class,
                        _communication.ToString())
                    .BindNamedParameter<ParameterClientConfig.SynchronizationType, string>(
                        GenericType<ParameterClientConfig.SynchronizationType>.Class,
                        _synchronization.ToString())
                    .BindNamedParameter<ParameterClientConfig.TablesRowsColumns, string>(
                        GenericType<ParameterClientConfig.TablesRowsColumns>.Class,
                        string.Join("|", _tablesRowsColumns.Select(rowsArr => string.Join(":", rowsArr))));
            return nameAssignments.Aggregate(configurationBuilder,SerializeNameAssignment).Build();
        }

        private static ICsConfigurationBuilder SerializeNameAssignment(ICsConfigurationBuilder builder, NameAssignment assignment)
        {
            var idAddressPort = new IdAddressPort(assignment.Identifier, new AddressPort(assignment.Endpoint));
            return builder.BindSetEntry<ParameterClientConfig.ComponentAddresses, string>(idAddressPort.ToString());
        }
    }
}