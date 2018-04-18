using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Docker.AppSDK
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DependencyAttribute : Attribute
    {

        public static IEnumerable<AppAnalyzer> GetAppDependencies(IApp app)
        {
            foreach (var pi in app.GetType().GetRuntimeProperties()) {
                if (pi.GetCustomAttribute<DependencyAttribute>() != null) {
                    var dep = pi.GetValue(app) as IApp;
                    yield return new AppAnalyzer(dep, nameOverride: pi.Name);
                }
            }
        }
    }
}
