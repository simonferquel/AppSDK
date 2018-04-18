using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Docker.AppSDK
{
    public class Parameter
    {
        private readonly IApp _app;
        private readonly PropertyInfo _pi;
        private readonly ParameterAttribute _paramAttr;

        public bool IsSet { get; private set; }

        public Parameter(IApp app, PropertyInfo pi) {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _pi = pi ?? throw new ArgumentNullException(nameof(pi));
            _paramAttr = pi.GetCustomAttribute<ParameterAttribute>() ?? throw new ArgumentException($"{pi.Name} has no ParameterAttribute", "pi");
        }

        public string Name => _paramAttr.Name;
        public bool Mandatory => _paramAttr.Mandatory;
        public string Description => _paramAttr.Description;

        public void Set(string strValue)
        {
            // convert to target type
            var typedVal = Convert.ChangeType(strValue, _pi.PropertyType, CultureInfo.InvariantCulture);
            _pi.SetValue(_app, typedVal);
            IsSet = true;
        }

        public string Get() => _pi.GetValue(_app)?.ToString();

        public static IEnumerable<Parameter> GetAppParameters(IApp app)
        {
            foreach (var pi in app.GetType().GetRuntimeProperties()) {
                if (pi.GetCustomAttribute<ParameterAttribute>() != null) {
                    yield return new Parameter(app, pi);
                }
            }
        }
    }
}
