using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docker.WebStore.Models
{
    public class ConfigureAppModel
    {
        public class Parameter
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Type Type { get; set; }
            public string Description { get; set; }
            public bool Mandatory { get; set; }
        }


        public class ServiceRuntimeSettings
        {
            public string Name { get; set; }
            public bool Scalable { get; set; }
            public int Scale { get; set; }
            public bool SetLimits { get; set; }
            public int CpuLimit { get; set; }
            public int MemoryLimit { get; set; }
        }

        public string Name { get; set; }
        public Type AppType { get; set; }    
        public List<Parameter> Parameters { get; } = new List<Parameter>();
        public List<ConfigureAppModel> Dependencies { get; } = new List<ConfigureAppModel>();
        public List<ServiceRuntimeSettings> RuntimeSettings { get; } = new List<ServiceRuntimeSettings>();
        public ConfigureAppModel() { }

        public string DeploymentName { get; set; }

        public void ApplyTo(AppAnalyzer app)
        {
            var settable = app.Parameters.ToList();
            for(var i =0;i<Parameters.Count; i++) {
                settable[i].Set(Parameters[i].Value);
            }
            var deps = app.Dependencies.ToList();
            for(var i=0;i<Dependencies.Count; i++) {
                Dependencies[i].ApplyTo(deps[i]);
            }
        }

        public async Task InitializeAsync(AppAnalyzer app)
        {
            Name = app.Name;
            AppType = app.App.GetType();
            foreach (var p in app.Parameters) {
                Parameters.Add(new Parameter {
                    Description = p.Description,
                    Mandatory = p.Mandatory,
                    Name = p.Name,
                    Type = p.ParameterType,
                    Value = p.Get()
                });
            }

            foreach (var dep in app.Dependencies) {
                var m = new ConfigureAppModel();
                await m.InitializeAsync(dep);
                Dependencies.Add(m);
            }

            var appBuilder = new AppBuilder();
            await app.Build(appBuilder);
            foreach(var svc in appBuilder.BuiltApp.Services) {
                RuntimeSettings.Add(
                    new ServiceRuntimeSettings {
                        Name = svc.Name,
                        Scalable = svc.ScalabilityModel == ScalabilityModel.Scalable,
                        Scale = 1,                        
                    });
            }


        }


        
    }
}
