using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Docker.AppSDK
{
    public class AppAnalyzer
    {
        private readonly IApp _app;
        private readonly string _nameOverride;
        private readonly Dictionary<string, Parameter> _params;
        private readonly Dictionary<string, AppAnalyzer> _dependencies;

        public AppAnalyzer(IApp app, string nameOverride = null)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _nameOverride = nameOverride;
            _params = Parameter.GetAppParameters(app).ToDictionary(p => p.Name);
            _dependencies = DependencyAttribute.GetAppDependencies(app).ToDictionary(p => p.Name);
        }

        private AppAttribute AppAttribute => _app.GetType().GetCustomAttributes(false).OfType<AppAttribute>().FirstOrDefault();
        public string Name => _nameOverride ?? OriginalName;
        public string OriginalName => AppAttribute?.Name ?? _app.GetType().Name;
        public Version Version => AppAttribute?.Version ?? new Version(0, 0, 0, 0);
        public string Author => AppAttribute?.Author;
        public string Description => AppAttribute?.Description;
        public IEnumerable<Parameter> Parameters => _params.Values;
        public Parameter GetParameter(string name) => _params[name];
        public IEnumerable<AppAnalyzer> Dependencies => _dependencies.Values;
        public AppAnalyzer GetDependency(string name) => _dependencies[name];

        public IApp App => _app;

        internal void ApplyParameterValues(string parameterValues)
        {
            if (string.IsNullOrEmpty(parameterValues)) {
                return;
            }
            var yaml = new YamlStream();
            yaml.Load(new StringReader(parameterValues));
            var mapping = yaml.Documents[0].RootNode as YamlMappingNode;
            ApplyParameterValues((YamlMappingNode) mapping[new YamlScalarNode("settings")]);
        }

        private void ApplyParameterValues(YamlMappingNode mapping)
        {
            foreach(var child in mapping.Children) {
                var key = ((YamlScalarNode)child.Key).Value;
                if (child.Value.NodeType == YamlNodeType.Scalar) {
                    GetParameter(key).Set(((YamlScalarNode)child.Value).Value);
                } else if (child.Value.NodeType == YamlNodeType.Mapping) {
                    GetDependency(key).ApplyParameterValues((YamlMappingNode)child.Value);
                }
            }
        }

        public async Task Build(AppBuilder appBuilder)
        {
            // starts with deps
            foreach(var dep in Dependencies) {
                await dep.Build(appBuilder);
            }
            await App.Build(appBuilder);
        }

        public string ParametersTemplate
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("settings:\n");
                var prefix = "  ";
                PrintAppParamsYaml(prefix, sb);
                return sb.ToString();
            }
        }

        private void PrintAppParamsYaml(string prefix, StringBuilder w)
        {
            foreach (var p in Parameters) {
                if (!string.IsNullOrEmpty(p.Description)) {
                    w.Append($"{prefix}# {p.Description}\n");
                }
                w.Append($"{prefix}{p.Name}: ");
                if (!string.IsNullOrEmpty(p.Get())) {
                    w.Append($"# default: {p.Get()}");
                }
                w.Append("\n");
            }
            foreach (var dep in Dependencies) {
                w.Append($"{prefix}{dep.Name}:\n");
                dep.PrintAppParamsYaml(prefix + "  ", w);
            }
        }
    }
}
