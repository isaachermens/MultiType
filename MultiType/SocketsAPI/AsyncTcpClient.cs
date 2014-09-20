using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using MultiType.AppData;
using MultiType.Models;
using MultiType.ViewModels;

namespace MultiType.SocketsAPI
{
    public class StatsReceivedEventArgs : EventArgs
    {
        public UserStatistics StatsUpdate { get; set; }
    }

    public class ContentReceivedEventArgs : EventArgs
    {
        public string Content { get; set; }
    }

    public interface IGameController
    {
        Action<bool> SetPauseState { get; set; }
        Action<string,bool> NewLesson { get; set; }
        Action<bool> RepeatLesson { get; set; }
    }

    public interface IAsyncTcpClient : IUpdateContent, IUpdateStats, IGameController
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
    }

    public interface IUpdateStats
    {
        event EventHandler<StatsReceivedEventArgs> StatsReceived;

        void OnStatsReceived(UserStatistics s);
    }

    public interface IUpdateContent
    {
        event EventHandler<ContentReceivedEventArgs> ContentReceived;

        void OnContentReceived(string content);
    }

    // Todo move application specific logic/functionality into a service class of some sort
    public class AsyncTcpClient : IAsyncTcpClient
    {
        private readonly TcpClient _tcpClient;
        // todo burn with fire
        public SerializeBase ReadData { get; set; }

        public Action<bool> SetPauseState { get; set; }
        public Action<string,bool> NewLesson { get; set; }
        public Action<bool> RepeatLesson { get; set; }

        public event EventHandler<StatsReceivedEventArgs> StatsReceived = delegate { };
        public void OnStatsReceived(UserStatistics s)
        {
            StatsReceived(this, new StatsReceivedEventArgs { StatsUpdate = s });
        }

        public event EventHandler<ContentReceivedEventArgs> ContentReceived = delegate{};
        public void OnContentReceived(string content)
        {
            ContentReceived(this, new ContentReceivedEventArgs { Content = content });
        }

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
            SetPauseState = delegate { };
            NewLesson = delegate { };
            RepeatLesson = delegate { };
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
            // todo can we replace the fields being checked with the is operator?
            if (packet.IsUserStatictics)
            { // use the data contained in the stats packet to update the peer databound properties in the view model
                var stats = (UserStatistics)packet;
                OnStatsReceived(stats);
                OnContentReceived(stats.TypedContent);
            }
            else if (packet.IsCommand)
            {
                var command = (Command)packet;
                //if (command.IsGameComplete) // alert the model that the game is complete
                //    // todo fix Model.GameIsComplete(false);
                //else if (command.IsPauseCommand)
                //{
                //    // todo fix Model.TogglePauseMulti(false);
                //    SetPauseState(command.GameHasStarted);
                //}
                //else if (command.StartCommand)
                //    // todo fix Model.StartGame(false, null); //command.StartTime, command.StopTime);
                //else if (command.IsResetCommand && command.ResetIsNewLesson)
                //{
                //    //_model.SendStatsPacket();
                //    // clear the lesson string and wait until the new lesson string is received from teh server
                //    NewLesson(command.LessonText, false);
                //}
                //else if (command.IsResetCommand && command.ResetIsRepeatedLesson)
                //{
                //    //_model.SendStatsPacket();
                //    RepeatLesson(false);
                //}
            }
        }
    }
}
