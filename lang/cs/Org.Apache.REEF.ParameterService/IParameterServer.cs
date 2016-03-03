using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.Apache.REEF.Tang.Annotations;

namespace Org.Apache.REEF.ParameterService
{
    [NamedParameter("The id to use to register a parameter server with the Name Service")]
    public class ParameterServerId : Name<string> { }

    [DefaultImplementation(typeof(ParameterServer))]
    interface IParameterServer : IParameterServiceComponent
    {
    }
}
