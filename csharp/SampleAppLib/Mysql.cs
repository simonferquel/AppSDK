using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleAppLib
{
    [App("mysql", "0.1.0")]
    public class Mysql:IApp
    {
        [Parameter("service-name-prefix", Description = "prefix applied to all service names")]
        public string ServiceNamePrefix { get; set; } = "";

        [Parameter("db-username", Mandatory = true)]
        public string DBUsername { get; set; }

        [Parameter("db-password", Mandatory = true)]
        public string DBPassword { get; set; }

        public async Task Build(IAppBuilder builder)
        {
            builder.AddService(ServiceNamePrefix + "db", "mysql:5.7",
                             s => s.WithScalabilityModel(ScalabilityModel.Singleton));
        }
    }
}
