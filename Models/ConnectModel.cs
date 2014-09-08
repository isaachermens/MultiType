using System;
using MultiType.SocketsAPI;
using MultiType.ViewModels;
using System.Net;
using System.Net.Sockets;

namespace MultiType.Models
{
	class ConnectModel
	{
		private ConnectVm _vm;

		internal ConnectModel(ConnectVm vm)
		{
			//store the reference to the view model
			_vm = vm;
		}

		/// <summary>
		/// Attempt to connect to the hosting application given an IP Address and port #.
		/// If an error occurs, sets text in an error div in the ClientConnect window using the InputError databount property.
		/// </summary>
		/// <param name="ipAddr">IP address of the host.</param>
		/// <param name="port">Port number of the host application.</param>
		internal void ConnectToServer(string ipAddr, string port)
		{
			//initiallize the error string
			const string defaultError = "An error has occured."; 
			var errorString = defaultError;
			int portNumber = 0;
			// if the inputted port cannot be parsed to an int, is in the reserved port range, or is too large to be a valid port number
			// modify error string
			if (!Int32.TryParse(port, out portNumber) || portNumber < 1024 || portNumber > 65535)
				errorString += "\nPlease enter a port number in the range 1024...65535";
			// modify error if the provided IP address cannot be parsed to an IPAddress
			var parsedIp = default(IPAddress);
			if (!IPAddress.TryParse(ipAddr, out parsedIp))
				errorString += "\nPlease enter an IP address in the form 'xxx.xxx.xxx.xxx'. Leading 0's in each segment may be omitted.";
			// if the error string has changed since initiallization, set error text through databound property.
			if (errorString != defaultError)
				_vm.InputError = errorString;
			else //no input errors
			{
				try
				{
					// initiallize an asynchronous socket and alert the GUI that a connection has been established, 
					// using the ConnectionEstablished databound property.
					_vm.asyncSocket = new AsyncTcpClient(ipAddr, portNumber);
					_vm.ConnectionEstablished = true;
				}
				catch (SocketException e)
				{ // something went wrong, set error text
					_vm.InputError = "A connection could not be established, please check your input values.";
				}
			}

		}
	}
}
