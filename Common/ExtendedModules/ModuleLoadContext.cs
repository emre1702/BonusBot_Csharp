using System.Reflection;
using System.Runtime.Loader;

namespace BonusBot.Common.ExtendedModules
{
    public sealed class ModuleLoadContext : AssemblyLoadContext
    {        
        public ModuleLoadContext() : base(true)
        {
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
    }
}
