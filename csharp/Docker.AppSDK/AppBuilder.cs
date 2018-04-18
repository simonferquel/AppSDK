using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppSDK
{
    public class AppBuilder : IAppBuilder
    {
        public IAppBuilder AddService(string name, string image, params Action<IServiceBuilder>[] options)
        {
            var svc = new Service { Name = name, Image = image, ScalabilityModel = ScalabilityModel.Scalable };
            BuiltApp.Services.Add(svc);
            var svcBuilder = new ServiceBuilder(svc);
            foreach(var opt in options) {
                opt(svcBuilder);
            }
            return this;
        }

        public App BuiltApp { get; } = new App();
    }
}
