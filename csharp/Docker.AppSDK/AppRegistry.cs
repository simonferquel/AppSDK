using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Docker.AppSDK
{
    public class AppRegistry
    {
        private Dictionary<string, Type> _apps = new Dictionary<string, Type>();
        private Dictionary<Type, string[]> _reverseDictionary = new Dictionary<Type, string[]>();
        public AppRegistry(IList<Assembly> asms)
        {
            var appNames = new SortedSet<string>();
            foreach (var asm in asms) {
                var asmName = asm.GetName().Name;
                foreach (var type in asm.GetTypes().Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IApp)))) {
                    var app = Activator.CreateInstance(type) as IApp;
                    var appAnalyzer = new AppAnalyzer(app);
                    var fullName = $"{asmName}.{appAnalyzer.Name}";
                    _apps[fullName] = type;
                    appNames.Add(fullName);
                    if (appNames.Contains(appAnalyzer.Name)) {
                        _apps.Remove(appAnalyzer.Name);
                    } else {
                        appNames.Add(appAnalyzer.Name);
                        _apps[appAnalyzer.Name] = type;
                    }
                }
            }
            foreach (var kvp in _apps) {
                if (_reverseDictionary.TryGetValue(kvp.Value, out var key)) {
                    var newKey = key.Append(kvp.Key).OrderBy(s => s.Length).ToArray();
                    _reverseDictionary[kvp.Value] = newKey;
                } else {
                    _reverseDictionary[kvp.Value] = new string[] { kvp.Key };
                }
            }
        }

        public bool TryGetValue(string name, out AppAnalyzer app) {
            if(_apps.TryGetValue(name, out var type)) {
                var appInstance = Activator.CreateInstance(type) as IApp;
                app = new AppAnalyzer(appInstance);
                return true;
            }
            app = null;
            return false;
        }

        public bool TryGetMeta(string name, out AppMeta meta)
        {
            meta = null;
            AppAnalyzer app = null;
            if(!TryGetValue(name, out app)) {
                return false;
            }
            string[] names = null;
            if(!TryGetNames(app, out names)) {
                return false;
            }
            meta = new AppMeta {
                Author = app.Author ?? "",
                Version = app.Version?.ToString() ?? "",
                Description = app.Description??""
            };
            meta.Names.AddRange(names);
            foreach(var dep in app.Dependencies) {
                string[] depNames = null;
                if(!TryGetNames(dep, out depNames)) {
                    return false;
                }
                AppMeta depMeta = null;
                if(!TryGetMeta(depNames[0], out depMeta)) {
                    return false;
                }
                meta.Dependencies[dep.Name] = depMeta;
            }
            return true;
        }

        public ICollection<(string[] names, AppAnalyzer app)> List()
        {            
            var result = new SortedSet<(string[] names, AppAnalyzer app)>(Comparer<(string[] names, AppAnalyzer app)>.Create((lhs, rhs) => {
                return lhs.names[0].CompareTo(rhs.names[0]);
            }));
            foreach (var kvp in _reverseDictionary) {
                var appInstance = Activator.CreateInstance(kvp.Key) as IApp;
                result.Add((kvp.Value, new AppAnalyzer(appInstance)));
            }
            return result;
        }

        public ICollection<AppMeta> ListMeta()
        {
            var result = new SortedSet<AppMeta>(Comparer<AppMeta>.Create((lhs, rhs) => {
                return lhs.Names[0].CompareTo(rhs.Names[0]);
            }));

            foreach (var kvp in _reverseDictionary) {
                if(TryGetMeta(kvp.Value[0], out var meta)) {
                    result.Add(meta);
                } else {
                    throw new InvalidOperationException("Unresolvable metadata");
                }
            }
            return result;
        }

        public bool TryGetNames(AppAnalyzer app, out string[] names) => _reverseDictionary.TryGetValue(app.App.GetType(), out names);
    }
}
