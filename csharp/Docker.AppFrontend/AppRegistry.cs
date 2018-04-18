using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Docker.AppFrontend
{
    static class AppRegistry
    {
        private static Docker.AppSDK.AppRegistry _registry;

        public static AppSDK.AppRegistry InnerRegistry { get { return _registry; } }
        internal static void Initialize(IList<Assembly> asms)
        {
            _registry = new AppSDK.AppRegistry(asms);
        }

        public static AppAnalyzer SelectedApp { get; private set; }

        internal static void Select(string name)
        {
            if(_registry.TryGetValue(name, out var app)) {
                SelectedApp = app;
            } else {
                throw new InvalidOperationException($"app \"{name}\" not found");
            }
            
        }

        internal static ICollection<(string[] names, AppAnalyzer app)> List()
        {
            return _registry.List();
        }
    }
}
