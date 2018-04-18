using Docker.AppSDK;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Docker.AppFrontend
{
    class ServeCommand:ICommand
    {
        private int _port = 42424;

        public string Name => "serve";
        public string Description => "Server frontend using grpc";

        public void Execute()
        {
            var frontend = new AppFrontendImpl(AppRegistry.InnerRegistry);
            var serviceDef = AppSDK.AppFrontend.BindService(frontend);
            var srv = new Grpc.Core.Server {
                Services = { serviceDef },
                Ports = { new ServerPort("0.0.0.0", _port, ServerCredentials.Insecure) }
            };
            srv.Start();
            Console.WriteLine($"Server is listening on {_port}. Press [enter] to exit");
            Console.ReadLine();
            srv.ShutdownAsync().Wait();                
        }
        public void PreExecute() => throw new NotImplementedException();

        public IEnumerable<ICommand> SubCommands => null;
        public IEnumerable<Flag> PositionalArguments => null;
        public IEnumerable<Flag> NamedFlags
        {
            get
            {
                yield return new Flag("port", p => _port = int.Parse(p, CultureInfo.InvariantCulture), "port on which to listen", shortHand: 'p');
             
            }
        }
    }
}
