using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Docker.AppSDK
{
    public interface IApp
    {
        Task Build(IAppBuilder builder);
    }
}
