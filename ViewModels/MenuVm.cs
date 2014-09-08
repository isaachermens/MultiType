using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MultiType.Commands;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
    public class MenuVm
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

            JoinGameCommand = new LambdaCommand(JoinGame);
            HostGameCommand = new LambdaCommand(HostGame);
            PlayAloneCommand = new LambdaCommand(PlayAlone);
            CloseAppCommand = new LambdaCommand(CloseApp);
        }


        private static void CloseApp()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Open the lesson select window to allow the user to host a game
        /// </summary>
        private void HostGame()
        {
            var lessonSelect = new LessonSelect(_host);
            lessonSelect.ShowDialog();
        }

        /// <summary>
        /// Open the client connect window to allow the user to connect to a host
        /// </summary>
        private void JoinGame()
        {
            var window = new ClientConnect(_host);
            window.ShowDialog();
            _host.Close();
        }

        /// <summary>
        /// Open the lesson select window to allow the user to begin a single player game
        /// </summary>
        private void PlayAlone()
        {
            var window = new LessonSelect(_host, isSinglePlayer: true);
            window.ShowDialog();
            _host.Close();
        }
    }
}
