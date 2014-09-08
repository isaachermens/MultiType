using System;
using System.Windows;
using MultiType.Commands;
using PropertyChanged;

namespace MultiType.ViewModels
{
    public class MenuVm : BaseVm
    {
        private readonly Window _host;

        public LambdaCommand JoinGameCommand { get; set; }
        public LambdaCommand HostGameCommand { get; set; }
        public LambdaCommand PlayAloneCommand { get; set; }
        public LambdaCommand CloseAppCommand { get; set; }

        public MenuVm(Window host)
        {
            // Caller must provide a reference to the host window
            if (host == null)
            {
                throw new ArgumentNullException();
            }
            _host = host;

            JoinGameCommand = new LambdaCommand(()=>new ClientConnect(_host));
            HostGameCommand = new LambdaCommand(()=>new LessonSelect(_host));
            PlayAloneCommand = new LambdaCommand(()=>ShowWindowAsDialog(new LessonSelect(_host, true)));
            CloseAppCommand = new LambdaCommand(()=>Application.Current.Shutdown());
        }
    }
}
