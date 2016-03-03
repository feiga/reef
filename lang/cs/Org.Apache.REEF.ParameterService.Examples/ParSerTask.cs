using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.Apache.REEF.Common.Tasks;
using Org.Apache.REEF.Tang.Annotations;
using Org.Apache.REEF.Utilities.Logging;

namespace Org.Apache.REEF.ParameterService.Examples
{
    class ParSerTask : ITask
    {
        private readonly IParameterServiceClient _paramServiceClient;
        private static readonly Logger Logger = Logger.GetLogger(typeof(ParSerTask));

        [Inject]
        public ParSerTask(IParameterServiceClient paramServiceClient)
        {
            _paramServiceClient = paramServiceClient;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public byte[] Call(byte[] memento)
        {
            throw new NotImplementedException();
        }
    }
}
