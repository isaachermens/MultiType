using System;
using System.Windows;
using MultiType.Commands;
using PropertyChanged;

namespace MultiType.ViewModels
{
    public class MenuVm : BaseVm
    {
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
            HostWindow = host;

            JoinGameCommand = new LambdaCommand(() => ShowWindowAsDialog(new ClientConnect(HostWindow)));
            HostGameCommand = new LambdaCommand(() => ShowWindowAsDialog(new LessonSelect(HostWindow)));
            PlayAloneCommand = new LambdaCommand(() => ShowWindowAsDialog(new LessonSelect(HostWindow, true)));
            CloseAppCommand = new LambdaCommand(()=>Application.Current.Shutdown());
        }
    }
}
