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
        void Add(int tableId, long rowId, float[] pDelta, float coeff);

        void Add(int tableId, long rowId, int[] pDelta, float coeff);

        void AsyncGet(int tableId, long[] rows, float[][] pValues);

        void AsyncGet(int tableId, long[] rows, int[][] pValues);

        void Barrier();

        long BatchLoad(int tableId, long[] rows);

        void Clock();

        void Get(int tableId, long rowId, float[] pValue);

        void Get(int tableId, long rowId, int[] pValue);

        void Set(int tableId, long rowId, float[] pValue);

        void Set(int tableId, long rowId, int[] pValue);
    }
}