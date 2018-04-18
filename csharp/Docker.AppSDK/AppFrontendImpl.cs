using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Docker.AppSDK
{
    public class AppFrontendImpl : AppFrontend.AppFrontendBase
    {
        private readonly AppRegistry _registry;

        public AppFrontendImpl(AppRegistry registry)
        {
            _registry = registry;
        }

        public override Task<AppMeta> GetApp(StringMessage request, ServerCallContext context)
        {
            if (_registry.TryGetMeta(request.Data, out var appMeta)) {
                return Task.FromResult(appMeta);
            }
            throw new InvalidOperationException($"App {request.Data} not found");
        }

        public override Task<ListAppResponse> ListApps(StringMessage request, ServerCallContext context){
            var metas = _registry.ListMeta();
            return Task.FromResult(new ListAppResponse { Apps = { metas } });
        }

        public override Task<StringMessage> GetAppSettingsTemplate(StringMessage request, ServerCallContext context) {
            if(_registry.TryGetValue(request.Data, out var app)) {
                return Task.FromResult(new StringMessage {
                    Data = app.ParametersTemplate
                });
            }
            throw new InvalidOperationException($"App {request.Data} not found");
        }

        public override async Task<App> RenderApp(RenderAppRequest request, ServerCallContext context) {
            if (_registry.TryGetValue(request.Name, out var app)) {
                app.ApplyParameterValues(request.ParameterValues);
                var appBuilder = new AppBuilder();
                await app.Build(appBuilder);
                return appBuilder.BuiltApp;
            }
            throw new InvalidOperationException($"App {request.Name} not found");
        }
    }
}
