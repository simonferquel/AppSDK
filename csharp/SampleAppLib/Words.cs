using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleAppLib
{
    [App("words", "1.0.0.0", Author = "Docker", Description = "Demo app with words")]
    public class Words : IApp
    {
        [Parameter("front-port", Description = "Port exposing frontal UI")]
        public int FrontalPort { get; set; } = 80;

        [Parameter("api-port", Description = "Port exposing api (if expose-api is true)")]
        public int ApiPort { get; set; } = 8888;

        [Parameter("expose-api", Description = "Exposes the API directly")]
        public bool ExposeAPI { get; set; } = false;

        public async Task Build(IAppBuilder builder)
        {
            builder.AddService("web", "dockerdemos/lab-web",
                s => s.WithExposedPort(80, publicPort: FrontalPort, kind: PortKind.Tcp))
                .AddService("words", "dockerdemos/lab-words",
                 s => {
                     if (ExposeAPI) {
                         s.WithExposedPort(8080, publicPort: ApiPort, kind: PortKind.Tcp);
                     }
                 })
                 .AddService("db", "dockerdemos/lab-db",
                 s=>s.WithScalabilityModel(ScalabilityModel.Singleton));
        }
    }
}
