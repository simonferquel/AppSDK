using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Docker.AppFrontend
{
    class RootCommand : ICommand
    {
        private IList<Assembly> _explicitAssemblies;
        public string Name { get; }

        public void Execute()
        {
            throw new NotImplementedException();
        }
        public void PreExecute()
        {
            var asms = _explicitAssemblies;
            if (asms == null) {
                asms = new List<Assembly>();
                foreach (var f in Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll")) {
                    try {
                        asms.Add(Assembly.LoadFrom(f));
                    } catch (Exception e) {
                        // dll does not seem to be an assembly
                    }
                }
            }
            AppRegistry.Initialize(asms);
        }
        public IEnumerable<ICommand> SubCommands
        {
            get
            {
                yield return new SelectCommand { };
                yield return new ListCommand { };
                yield return new ServeCommand { };
         
            }
        }
        public IEnumerable<Flag> PositionalArguments => null;

        private void ParseAssemblies(string assemblies)
        {
            List<Assembly> asms = new List<Assembly>();
            foreach (var asmName in assemblies.Split(",")) {
                try {
                    asms.Add(Assembly.LoadFrom(asmName));
                } catch (Exception e) {
                    throw new InvalidOperationException($"Unable to load assembly {asmName}", e);
                }
            }
            _explicitAssemblies = asms;
        }

        public IEnumerable<Flag> NamedFlags
        {
            get
            {
                yield return new Flag("assemblies", ParseAssemblies,
                    description: "comma seperated list of assemblies (default: all dlls in current dir)",
                    mandatory: false);
            }
        }

        public string Description => "Command line tool to handle .Net defined Apps";
    }
}
