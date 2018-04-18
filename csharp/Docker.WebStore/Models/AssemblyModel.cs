using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Docker.WebStore.Models
{
    public class AssemblyModel
    {
        private readonly Assembly _asm;

        public AssemblyModel(Assembly asm)
        {
            _asm = asm;
        }

        public string Name => _asm.GetName().Name;
        public Version Version => _asm.GetName().Version;
        public string FullName => _asm.FullName;

        public IEnumerable<AppAnalyzer> Apps => _asm.GetTypes()
                    .Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IApp)))
                    .Select(t => new AppAnalyzer((IApp)Activator.CreateInstance(t)))
                    .OrderBy(a => a.Name);
    }
}
