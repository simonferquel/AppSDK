using System;

namespace Docker.AppSDK
{
    internal class ServiceBuilder : IServiceBuilder
    {
        public ServiceBuilder(Service svc) => BuiltService = svc;

        public IServiceBuilder WithExposedPort(int containerPort, int publicPort = 0, PortKind kind = PortKind.Both)
        {
            BuiltService.Ports.Add(new ExposedPort {
                ContainerPort = containerPort,
                Kind = kind,
                PublicPort = publicPort,
            });
            return this;
        }
        public IServiceBuilder WithCommand(params string[] command)
        {
            BuiltService.Command.Clear();
            BuiltService.Command.AddRange(command);
            return this;
        }
        public IServiceBuilder WithEnv(string name, string value)
        {
            BuiltService.Env[name] = value;
            return this;
        }
        public IServiceBuilder WithScalabilityModel(ScalabilityModel scale)
        {
            BuiltService.ScalabilityModel = scale;
            return this;
        }

        public Service BuiltService { get; }
    }
}