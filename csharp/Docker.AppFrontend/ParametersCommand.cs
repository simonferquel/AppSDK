using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Docker.AppFrontend
{
    class ParametersCommand : ICommand
    {
        public string Name => "params";
        public string Description => "manipulate parameters of the selected app";

        bool _outputAsYaml;

        public void Execute()
        {
            
            if (_outputAsYaml) {
                Console.Write("settings:\n");
                var prefix = "  ";
                var app = AppRegistry.SelectedApp;
                PrintAppParamsYaml(prefix, app);

            } else { 
                var parameters = FlattenParameters(AppRegistry.SelectedApp, "").ToList();
                var formatter = new TableFormatter<(string prefix, Parameter param)>(
                    new ColumnDefinition<(string prefix, Parameter param)>("NAME", v => {
                        var sb = new StringBuilder(v.prefix);
                        sb.Append(v.param.Name);
                        if (v.param.Mandatory) {
                            sb.Append(" (required)");
                        }
                        return sb.ToString();
                    }),
                    new ColumnDefinition<(string prefix, Parameter param)>("DESCRIPTION", v => v.param.Description),
                new ColumnDefinition<(string prefix, Parameter param)>("DEFAULT", v => v.param.Get()));
                formatter.Print(parameters, Console.Out);
            }
        }

        private static void PrintAppParamsYaml(string prefix, AppAnalyzer app)
        {
            foreach (var p in app.Parameters) {
                if (!string.IsNullOrEmpty(p.Description)) {
                    Console.Write($"{prefix}# {p.Description}\n");
                }
                Console.Write($"{prefix}{p.Name}: ");
                if (!string.IsNullOrEmpty(p.Get())) {
                    Console.Write($"# default: {p.Get()}");
                }
                Console.Write("\n");
            }
            foreach(var dep in app.Dependencies) {
                Console.Write($"{prefix}{dep.Name}:\n");
                PrintAppParamsYaml(prefix + "  ", dep);                   
            }
        }


        IEnumerable<(string prefix, Parameter param)> FlattenParameters(AppAnalyzer app, string prefix)
        {
            foreach(var p in app.Parameters) {
                yield return (prefix, p);
            }
            foreach(var dep in app.Dependencies) {
                prefix = prefix.Length == 0 ? dep.Name+"." : $"{prefix}.{dep.Name}.";
                foreach(var p in FlattenParameters(dep, prefix)) {
                    yield return p;
                }
            }
        }
        public void PreExecute() => throw new NotImplementedException();

        public IEnumerable<ICommand> SubCommands => null;
        public IEnumerable<Flag> PositionalArguments => null;
        public IEnumerable<Flag> NamedFlags
        {
            get
            {
                yield return new Flag("as-yaml", v => _outputAsYaml = bool.Parse(v), shortHand: 'y', description: "output as a configuration yaml file", isSwitch: true);
            }
        }
    }
}
