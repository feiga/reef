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
using System.Security.Cryptography.X509Certificates;
using Org.Apache.REEF.Tang.Annotations;

namespace Org.Apache.REEF.ParameterService
{
    [DefaultImplementation(typeof(ParameterClient))]
    public interface IParameterClient : IDisposable
    {
        /// <summary>
        /// Get the whole table from parameter server
        /// </summary>
        /// <param name="tableId"> id of table to get </param>
        /// <param name="pValue"> array to store the returned parameter, 
        /// whose size should be the num_rows * num_cols of the specific table 
        /// </param>
        void Get(int tableId, int[] pValue);
        void Get(int tableId, float[] pValue);

        /// <summary>
        /// Get the specific row of the specific table from parameter server
        /// </summary>
        /// <param name="tableId"> id of table to get </param>
        /// <param name="rowId"> id of row to get </param>
        /// <param name="pValue"> array to store the returned parameter, 
        /// whose size should be the num_cols of specific table 
        /// </param>
        void Get(int tableId, long rowId, int[] pValue);
        void Get(int tableId, long rowId, float[] pValue);

        /// <summary>
        /// Add the updates of whole table to parameter server
        /// </summary>
        /// <param name="tableId"> id of table to add </param>
        /// <param name="pValue"> array of updates, size equals num_rows * num_cols </param>
        void Add(int tableId, int[] pValue);
        void Add(int tableId, float[] pValue);

        /// <summary>
        /// Add the updates of specific row id to parameter server
        /// </summary>
        /// <param name="tableId"> id of table to add </param>
        /// <param name="rowId"> id of row to add </param>
        /// <param name="pDelta"> array of updates, size equals num_cols </param>
        void Add(int tableId, int rowId, int[] pDelta);
        void Add(int tableId, int rowId, float[] pDelta);

        /// <summary>
        /// Barrier all the clients
        /// </summary>
        void Barrier();

        // NOTE(feiga): deprecate following API. This will make our API clean and simple
        // The BatchLoad is actually same with Get.
        // The Async API need user to control its memory to ensure the param valid until the 
        // Async call really finished. I suggest not to support this in the 
        // initial version. 
        // The Clock is useless. We can use Get/Add to do
        // ASP training, and use Barrier to support the BSP training.
        // Set is useless in ML. We use Get to get latest model, use Add to add local
        // updates to server. 
        // Int32 as row id should be enough
        //
        //long BatchLoad(int tableId, long[] rows);

        //void AsyncGet(int tableId, long[] rows, float[][] pValues);

        //void AsyncGet(int tableId, long[] rows, int[][] pValues);

        //void Set(int tableId, long rowId, float[] pValue);

        //void Set(int tableId, long rowId, int[] pValue);

        //void Clock();
    }
}