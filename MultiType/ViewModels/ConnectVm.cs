using System.Windows;
using MultiType.Commands;
using MultiType.Models;
using System.ComponentModel;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
	class ConnectVm
	{
		private readonly ConnectModel _model;
		internal SocketsAPI.AsyncTcpClient asyncSocket;

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

		internal ConnectVm()
		{
			_model = new ConnectModel(this);
			IpAddress = "";
		}

		internal void ConnectToServer()
		{
			_model.ConnectToServer(IpAddress, PortNumber);
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
