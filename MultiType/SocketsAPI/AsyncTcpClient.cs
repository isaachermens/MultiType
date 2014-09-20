using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using MultiType.AppData;
using MultiType.Models;
using MultiType.ViewModels;

namespace MultiType.SocketsAPI
{

    public interface IAsyncTcpClient
    {
        /// <summary>
        /// Encodes a serializable object and writes it to the network stream
        /// </summary>
        /// <param name="data">The serializable object to serialize and write</param>
        void Write(object data);

        /// <summary>
        /// Writes an array of bytes to the network.
        /// </summary>
        /// <param name="bytes">The array to write</param>
        void Write(byte[] bytes);

        /// <summary>
        /// Callback for Write operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        void WriteCallback(IAsyncResult result);

        /// <summary>
        /// Start read operations
        /// </summary>
        void BeginReading();

        /// <summary>
        /// Callback for Read operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        void ReadCallback(IAsyncResult result);

        void ReadPacket(SerializeBase packet);
        SerializeBase ReadData { get; set; }
        IPacketParser Parser { get; set; }
    }

    public class AsyncTcpClient : IAsyncTcpClient
    {
        private readonly TcpClient _tcpClient;
        public IPacketParser Parser { get; set; }

        public SerializeBase ReadData { get; set; }


        /// <summary>
        /// Construct a new client from a provided IP address and port
        /// </summary>
        /// <param name="address">The IP Address of the server</param>
        /// <param name="port">The port of the server</param>
        internal AsyncTcpClient(string address, int port)
            : this(new TcpClient(address, port))
        {
        }

        /// <summary>
        /// construct an async client given a standard TcpClient
        /// </summary>
        internal AsyncTcpClient(TcpClient client)
        {
            _tcpClient = client;
        }

        /// <summary>
        /// Encodes a serializable object and writes it to the network stream
        /// </summary>
        /// <param name="data">The serializable object to serialize and write</param>
        public void Write(object data)
        {
            var bytes = Serializer.SerializeToByteArray(data);
            Write(bytes);
        }

        /// <summary>
        /// Writes an array of bytes to the network.
        /// </summary>
        /// <param name="bytes">The array to write</param>
        public void Write(byte[] bytes)
        {
            if (_tcpClient.Client.Connected == false) return;
            var networkStream = _tcpClient.GetStream();
            //Start async write operation
            networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, null);
        }

        /// <summary>
        /// Callback for Write operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        public void WriteCallback(IAsyncResult result)
        {
            var networkStream = _tcpClient.GetStream();
            networkStream.EndWrite(result);
        }

        /// <summary>
        /// Start read operations
        /// </summary>
        public void BeginReading()
        {
            var networkStream = _tcpClient.GetStream();
            var buffer = new byte[_tcpClient.ReceiveBufferSize];
            //Now we are connected start asyn read operation.
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        /// <summary>
        /// Callback for Read operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        public void ReadCallback(IAsyncResult result)
        {
            int read;
            NetworkStream networkStream;
            try
            {
                networkStream = _tcpClient.GetStream();
                read = networkStream.EndRead(result);
            }
            catch
            {
                return;
            }
            if (read == 0)
            {
                //The connection has been closed.
                return;
            }

            var buffer = result.AsyncState as byte[];
            ReadData = Serializer.DeserializeFromByteArray(buffer);
            //readData = Serializer.DeserializeFromByteArray(this._encoding.GetString(buffer, 0, read));
            // process the packet
            if (ReadData != null) ReadPacket(ReadData);
            //Then start another async read operation
            if (buffer != null)
                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        public void ReadPacket(SerializeBase packet)
        {
            if (Parser == null) return;
            Parser.HandlePacket(packet);
        }
    }
}
