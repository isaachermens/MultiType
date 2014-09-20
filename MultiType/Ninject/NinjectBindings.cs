using MultiType.Services;
using MultiType.SocketsAPI;
using Ninject.Modules;

namespace MultiType.Ninject
{
    public class NinjectBindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ILessonManagementService>().To<LessonManagementService>();
            Bind<ISocketConnectionService>().To<SocketConnectionService>();
            Bind<IAsyncTcpClient>().To<AsyncTcpClient>();
        }
    }
}
