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
using System.Globalization;
using System.Linq;
using System.Threading;
using Org.Apache.REEF.Common.Tasks;
using Org.Apache.REEF.Driver;
using Org.Apache.REEF.Driver.Context;
using Org.Apache.REEF.Driver.Evaluator;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Tang.Implementations.Configuration;
using Org.Apache.REEF.Tang.Util;
using Org.Apache.REEF.Utilities.Logging;

namespace Org.Apache.REEF.ParameterService.Examples
{
    class ParSerDriver : IObserver<IAllocatedEvaluator>,
        IObserver<IActiveContext>,
        IObserver<IDriverStarted>
    {
        private static readonly Logger LOGGER = Logger.GetLogger(typeof(ParSerDriver));

        private readonly IParameterService _parService;

        private int _taskCounter;

        [Inject]
        public ParSerDriver(ParameterServiceBuilder parameterServiceBuilder)
        {
            var tablesRowsColumns = new int[1][];
            tablesRowsColumns[0] = Enumerable.Repeat(1000, 1000).ToArray();
            _parService =
                parameterServiceBuilder.SetCommunicationType(CommunicationType.Reduce)
                    .SetSynchronizationType(SynchronizationType.Average)
                    .SetElementType(ElementType.Single)
                    .SetNumberOfTasks(5)
                    .SetTaskCores(2)
                    .SetTaskMemoryMB(512)
                    .SetTablesRowsColumns(tablesRowsColumns)
                    .Build();
        }


        public void OnNext(IDriverStarted value)
        {
            _parService.RequestEvaluators();
        }

        public void OnNext(IAllocatedEvaluator allocatedEvaluator)
        {
            var localContextConf = _parService.GetLocalContextConfiguration();
            var sharedContextConf = _parService.GetParameterServiceConfiguration();
            allocatedEvaluator.SubmitContextAndService(localContextConf, sharedContextConf);
        }

        public void OnNext(IActiveContext activeContext)
        {
            var taskId = string.Format(CultureInfo.InvariantCulture, "Task-{0}", Interlocked.Increment(ref _taskCounter));
            var partialTaskConfig = TaskConfiguration.ConfigurationModule
                .Set(TaskConfiguration.Identifier, taskId)
                .Set(TaskConfiguration.Task, GenericType<ParSerTask>.Class)
                .Build();
            _parService.QueueTask(partialTaskConfig, activeContext);
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnCompleted()
        {
        }
    }
}
