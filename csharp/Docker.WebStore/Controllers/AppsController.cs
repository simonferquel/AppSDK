using Docker.AppSDK;
using Docker.WebStore.Models;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Docker.WebStore.Controllers
{
    [Route("apps")]
    public class AppsController : Controller
    {
        private readonly BackendService _backend;
        private readonly Store _store;

        public AppsController(BackendService backend, Store store)
        {
            _backend = backend;
            _store = store;
        }

        private Type ResolveAppType(string fullName)
        {
            var asmName = fullName.Substring(fullName.IndexOf(",")+2);
            var typeName = fullName.Substring(0, fullName.IndexOf(","));
            var asm = _store.List().First(a => a.FullName == asmName);
            return asm.GetType(typeName);
        }

        [Route("configure/{fullTypeName}")]
        public async Task<IActionResult> Configure(string fullTypeName)
        {
            var app = (IApp)Activator.CreateInstance(ResolveAppType(fullTypeName));
            var analyzer = new AppAnalyzer(app);
            var model = new ConfigureAppModel();
            await model.InitializeAsync(analyzer);
            return View(model);
        }

        [HttpPost]
        [Route("runtime-settings/{fullTypeName}")]
        public async Task<IActionResult> RuntimeSettings(string fullTypeName, ConfigureAppModel model)
        {
            var app = (IApp)Activator.CreateInstance(ResolveAppType(fullTypeName));
            var analyzer = new AppAnalyzer(app);
            model.ApplyTo(analyzer);
            model = new ConfigureAppModel();
            await model.InitializeAsync(analyzer);
            return View(model);
        }

        [HttpPost]
        [Route("deploy/{fullTypeName}")]
        public async Task<IActionResult> Deploy(string fullTypeName, ConfigureAppModel model)
        {

            var app = (IApp)Activator.CreateInstance(ResolveAppType(fullTypeName));
            var analyzer = new AppAnalyzer(app);
            model.ApplyTo(analyzer);

            var appBuilder = new AppBuilder();
            await analyzer.Build(appBuilder);

            var restoredModel = new ConfigureAppModel();
            await restoredModel.InitializeAsync(analyzer);
            restoredModel.RuntimeSettings.Clear();
            restoredModel.RuntimeSettings.AddRange(model.RuntimeSettings);
            // generate parameters
            var parametersYaml = GenerateParametersYaml(restoredModel);
            var runtimeSettingsYaml = GenerateRuntimeSettingsYaml(restoredModel);
            
 
            var svcFile = Path.GetTempFileName();
            var paramsFile = Path.GetTempFileName();
            var runtimeFile = Path.GetTempFileName();
            try {
                using(var svcStream = System.IO.File.Create(svcFile)){
                    appBuilder.BuiltApp.WriteTo(svcStream);
                    await svcStream.FlushAsync();
                }
                await System.IO.File.WriteAllTextAsync(paramsFile, parametersYaml, Encoding.UTF8);
                await System.IO.File.WriteAllTextAsync(runtimeFile, runtimeSettingsYaml, Encoding.UTF8);
                await _backend.RunAsync(model.DeploymentName,  svcFile, paramsFile, runtimeFile);
            } finally {
                System.IO.File.Delete(svcFile);
                System.IO.File.Delete(paramsFile);
                System.IO.File.Delete(runtimeFile);
            }

            return RedirectToAction("List", "AppLibs");
        }

        private string GenerateRuntimeSettingsYaml(ConfigureAppModel model)
        {
            var rootNode = new YamlMappingNode();
            var doc = new YamlDocument(rootNode);
            var appNode = new YamlMappingNode();
            rootNode.Add("runtime", appNode);
            
            foreach(var s in model.RuntimeSettings) {
                var svcNode = new YamlMappingNode();
                appNode.Add(s.Name, svcNode);
                if (s.Scale > 1) {
                    svcNode.Add("scale", s.Scale.ToString());
                } 
                if (s.SetLimits) {
                    var limitsNode = new YamlMappingNode();
                    svcNode.Add("limits", limitsNode);
                    if (s.MemoryLimit > 0) {
                        limitsNode.Add("memory", s.MemoryLimit.ToString());
                    }
                    if (s.CpuLimit > 0) {
                        limitsNode.Add("cpu", s.CpuLimit.ToString());
                    }
                }
            }

            var yamlStream = new YamlStream(doc);
            var parametersWriter = new StringWriter();            
            yamlStream.Save(parametersWriter, false);
            return parametersWriter.ToString();
        }

        private string GenerateParametersYaml(ConfigureAppModel model)
        {
            var rootNode = new YamlMappingNode();
            var doc = new YamlDocument(rootNode);
            var appNode = new YamlMappingNode();
            rootNode.Add("settings", appNode);
            PopulateParameters(appNode, model);
            var yamlStream = new YamlStream(doc);
            var parametersWriter = new StringWriter();
            yamlStream.Save(parametersWriter, false);
            return parametersWriter.ToString();
        }

        private void PopulateParameters(YamlMappingNode appNode, ConfigureAppModel model)
        {
            foreach(var p in model.Parameters) {
                appNode.Add(p.Name, p.Value);
            }
            foreach(var dep in model.Dependencies) {
                var depNode = new YamlMappingNode();
                PopulateParameters(depNode, dep);
                appNode.Add(dep.Name, depNode);
            }
        }
    }
}
