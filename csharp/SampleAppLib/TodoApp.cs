using Docker.AppSDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleAppLib
{
    [App("todo-list", "0.1.0", Author = "Simon Ferquel", Description = "simple todo list")]
    public class TodoApp : IApp
    {
        [Parameter("with-front")]
        public bool WithFront { get; set; } = true;

        [Parameter("expose-api")]
        public bool ExposeAPI { get; set; } = false;

        [Parameter("front-port")]
        public int FrontPublicPort { get; set; } = 80;

        [Parameter("api-port", Description = "port on which to expose the API (if expose-api is true)")]
        public int APIPublicPort { get; set; } = 4242;

        [Parameter("service-name-prefix", Description = "prefix applied to all service names")]
        public string ServiceNamePrefix { get; set; } = "todolist-";

        [Dependency]
        public Mysql DB { get; } = new Mysql { ServiceNamePrefix = "todolist-" };

        public async Task Build(IAppBuilder builder)
        {
            builder = builder.AddService(ServiceNamePrefix + "api", "todolist-api",
                              s => s.WithEnv("DB_HOSTNAME", DB.ServiceNamePrefix + "db"),
                              s => {
                                  if (ExposeAPI) {
                                      s.WithExposedPort(80, publicPort: APIPublicPort, kind: PortKind.Tcp);
                                  }
                              });
            if (WithFront) {
                builder.AddService(ServiceNamePrefix + "front", "todolist-front",
                        s => s.WithCommand("/todolist-front", "--api-path", $"http://{ServiceNamePrefix}api"),
                        s => s.WithExposedPort(80, publicPort: FrontPublicPort, kind: PortKind.Tcp));
            }
        }
    }
}
