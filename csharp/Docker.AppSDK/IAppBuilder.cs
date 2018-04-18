using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppSDK
{


    public interface IServiceBuilder
    {
        IServiceBuilder WithExposedPort(int containerPort, int publicPort =0, PortKind kind = PortKind.Both);
        IServiceBuilder WithCommand(params string[] command);
        IServiceBuilder WithEnv(string name, string value);
        IServiceBuilder WithScalabilityModel(ScalabilityModel scale);
        Service BuiltService { get; }
    }
    public interface IAppBuilder
    {
        IAppBuilder AddService(string name, string image, params Action<IServiceBuilder>[] options);
        App BuiltApp { get; }
    }
}
