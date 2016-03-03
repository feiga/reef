using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.Apache.REEF.Tang.Annotations;

namespace Org.Apache.REEF.ParameterService
{
    [NamedParameter("The id to use to register a parameter client with the Name Service")]
    public class ParameterClientId : Name<string> { }

    [DefaultImplementation(typeof(ParameterClient))]
    interface IParameterClient : IParameterServiceComponent
    {
    }
}
