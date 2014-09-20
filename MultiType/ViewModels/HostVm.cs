using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MultiType.SocketsAPI;
using MultiType.Windows;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
    public class HostVm : BaseVm
    {
        private static readonly ManualResetEvent TcpClientConnected = new ManualResetEvent(false);
        private readonly Window _host;
        private readonly string _lessonContent;

        public string IpAddress { get; set; }

        public string PortNumber { get; set; }

        public HostVm(Window host, string lessonContent)
        {
            _host = host;
            OpenListenSocket();
            _lessonContent = lessonContent;
        }

        internal void OpenListenSocket()
        {
            // Create a TCPListener socket to wait for a peer/client to connect
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            // extract the port number of the listener
            var endPoint = (IPEndPoint)listener.LocalEndpoint;
            PortNumber = endPoint.Port.ToString(CultureInfo.InvariantCulture);
            // get the local computer IPv4 address. This method of retrieving the IP address will have difficulties if a computer
            // has multiple IPv4 addresses
            var hostDns = Dns.GetHostEntry(Dns.GetHostName());
            var ip = hostDns.AddressList.FirstOrDefault(c => c.AddressFamily.ToString().Equals("InterNetwork"));
            if (ip == null) return; //todo throw an exception here?
            IpAddress = ip.ToString(); // set databound IPaddress property
            //Reset the ManualReset event and begin async op to accept connection request
            TcpClientConnected.Reset();
            listener.BeginAcceptTcpClient(AcceptClientConnection, listener);
        }

        public void AcceptClientConnection(IAsyncResult ar)
        {
            var listener = (TcpListener)ar.AsyncState; // cast the result to a TcpListener
            var client = listener.EndAcceptTcpClient(ar); // stop listening for clients
            var socket = new AsyncTcpClient(client); // create an asynchronous tcp socket using the tcp client socket.
            listener.Stop(); // kill the listener
            TcpClientConnected.Set(); // set the data bound
            ShowWindowAsDialog(_host, new TypingWindow(socket, _lessonContent, true));
        }
    }
}