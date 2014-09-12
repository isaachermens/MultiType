using System;
using System.Windows;
using MultiType.Commands;
using MultiType.Windows;
using PropertyChanged;

namespace MultiType.ViewModels
{
    public class MenuVm : BaseVm
    {
        public RelayCommand<Window> JoinGameCommand { get; set; }
        public RelayCommand<Window> HostGameCommand { get; set; }
        public RelayCommand<Window> PlayAloneCommand { get; set; }
        public LambdaCommand CloseAppCommand { get; set; }

        public MenuVm()
        {
            JoinGameCommand = new RelayCommand<Window>(w => ShowWindowAsDialog(w, new ClientConnect(w)));
            HostGameCommand = new RelayCommand<Window>(w => ShowWindowAsDialog(w, new LessonSelect(w)));
            PlayAloneCommand = new RelayCommand<Window>(w => ShowWindowAsDialog(w, new LessonSelect(w, true)));
            CloseAppCommand = new LambdaCommand(() =>Application.Current.Shutdown());
        }
    }
}
