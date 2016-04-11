using System;
using System.Linq;
using System.Reflection;

namespace ExplorerServer.Core.Network
{
    public class DeserializationBinder : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            typeToDeserialize = Type.GetType("ExplorerServer.Core.Network." + typeName.Split('.').Last());
            

            return typeToDeserialize;
        }
    }
}