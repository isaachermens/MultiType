using System;
using MultiType.SocketsAPI;
using MultiType.ViewModels;
using System.Net;
using System.Net.Sockets;

namespace MultiType.Services
{
	class SocketConnectionService
	{
	    private static bool IsValidPort(int port)
	    {
	        return port >= 1024 && port <= 65535;
	    }

	    /// <summary>
	    /// Attempt to connect to the hosting application given an IP Address and port #.
	    /// If an error occurs, sets text in an error div in the ClientConnect window using the InputError databount property.
	    /// </summary>
	    /// <param name="ipAddr">IP address of the host.</param>
	    /// <param name="port">Port number of the host application.</param>
	    /// <param name="socket">reference parameter to hold a sucessful socket connection</param>
	    /// <param name="errorMessage">Description of any errors that occur</param>
	    internal bool TryConnectToServer(string ipAddr, int port, ref AsyncTcpClient socket, ref string errorMessage)
		{
			//initiallize the error string
			const string defaultError = "An error has occured."; 
			var errorString = defaultError;
			// if the inputted port cannot be parsed to an int, is in the reserved port range, or is too large to be a valid port number
			// modify error string
            if (!IsValidPort(port))
				errorString += "\nPlease enter a port number in the range 1024...65535";
			// modify error if the provided IP address cannot be parsed to an IPAddress
			IPAddress parsedIp;
			if (!IPAddress.TryParse(ipAddr, out parsedIp))
				errorString += "\nPlease enter an IP address in the form 'xxx.xxx.xxx.xxx'. Leading 0's in each segment may be omitted.";
			// if the error string has changed since initiallization, set error text through databound property.
	        if (errorString != defaultError)
	        {
	            errorMessage = errorString;
	            return false;
	        }
	        else //no input errors
	        {
	            try
	            {
	                // initiallize an asynchronous socket and return true to indicate that a connection has been established
	                socket = new AsyncTcpClient(ipAddr, port);
	                return true;
	            }
	            catch (SocketException e)
	            {
	                // something went wrong, set error text
	                errorMessage = "A connection could not be established, please check your input values.";
	                return false;
	            }
	        }

		}
	}
}
