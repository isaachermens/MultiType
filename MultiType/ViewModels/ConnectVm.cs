using System.Windows;
using MultiType.Commands;
using MultiType.Models;
using System.ComponentModel;
using MultiType.Services;
using MultiType.Windows;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
	class ConnectVm : BaseVm
    {
        private readonly ISocketConnectionService _connectService;
		private SocketsAPI.AsyncTcpClient _socket;
        private readonly Window _host;

        public RelayCommand<Window> Cancel { get { return new RelayCommand<Window>(w =>
        {
            w.DialogResult = true;
            w.Close();
        });} }

        public LambdaCommand Connect { get { return new LambdaCommand(ConnectToServer);} }

		public string InputError { get; set; }

		public bool ConnectionEstablished { get; set; }

        public string IpAddress { get; set; }

        public int PortNumber { get; set; }

		internal ConnectVm(Window hostWindow, ISocketConnectionService service)
		{
		    _connectService = service;
			IpAddress = "";
		    _host = hostWindow;
		}

		internal void ConnectToServer()
		{
		    var error = string.Empty;
		    if (_connectService.TryConnectToServer(IpAddress, PortNumber, ref _socket, ref error))
		    {
		        ShowWindowAsDialog(_host, new TypingWindow("", _socket));
		    }
		    else
		    {
		        InputError = error;
		    }
		}

		#region NPC Implementation
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(string prop)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}
		}
		#endregion NPC Implementation
	}
}
