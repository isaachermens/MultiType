using System;
using System.Net.Sockets;
using MultiType.Models;
using MultiType.ViewModels;

namespace MultiType.SocketsAPI
{
	internal class AsyncTcpClient
	{
        private TcpClient _tcpClient;
		internal PrimaryVm _viewModel;
		internal PrimaryModel _model;
		internal SerializeBase readData;

        /// <summary>
        /// Construct a new client from a provided IP address and port
        /// </summary>
        /// <param name="address">The IP Address of the server</param>
        /// <param name="port">The port of the server</param>
        internal  AsyncTcpClient(string address, int port) : this(new TcpClient(address, port))
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
        internal  void Write(object data)
        {
            byte[] bytes = Serializer.SerializeToByteArray(data);
            Write(bytes);
        }

        /// <summary>
        /// Writes an array of bytes to the network.
        /// </summary>
        /// <param name="bytes">The array to write</param>
        internal  void Write(byte[] bytes)
        {
			if (_tcpClient.Client.Connected == false) return;
            NetworkStream networkStream = _tcpClient.GetStream();
            //Start async write operation
			networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, null);
        }

        /// <summary>
        /// Callback for Write operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void WriteCallback(IAsyncResult result)
        {
            NetworkStream networkStream = _tcpClient.GetStream();
            networkStream.EndWrite(result);
        }
        
		/// <summary>
		/// Start read operations
		/// </summary>
		internal void BeginReading()
		{
			NetworkStream networkStream = _tcpClient.GetStream();
			byte[] buffer = new byte[_tcpClient.ReceiveBufferSize];
			//Now we are connected start asyn read operation.
			networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
		}
 
        /// <summary>
        /// Callback for Read operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ReadCallback(IAsyncResult result)
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
 
            byte[] buffer = result.AsyncState as byte[];
			readData = Serializer.DeserializeFromByteArray(buffer);
			//readData = Serializer.DeserializeFromByteArray(this._encoding.GetString(buffer, 0, read));
            // process the packet
			if (readData != null) ReadPacket(readData);
            //Then start another async read operation
			networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

		private void ReadPacket(SerializeBase packet)
		{
			if (packet.IsUserStatictics)
			{ // use the data contained in the stats packet to update the peer databound properties in the view model
				var stats = (UserStatistics)packet;
				_viewModel.PeerCompletionPercentage = stats.CompletionPercentage;
				_viewModel.PeerTypedContent = stats.TypedContent;
				_viewModel.PeerCharactersTyped = stats.CharactersTyped.ToString();
				_viewModel.PeerAccuracy = stats.Accuracy;
				_viewModel.PeerErrors = stats.Errors.ToString();
				_viewModel.PeerWPM = stats.WPM.ToString();
			}
			else if (packet.IsCommand)
			{
				var command = (Command)packet;
				if (command.IsGameComplete) // alert the model that the game is complete
					_model.GameIsComplete(isLocalCall:false);
				else if (command.IsPauseCommand)
				{
					_model.TogglePauseMulti(false);
					_viewModel.gameHasStarted = command.GameHasStarted;
				}
				else if (command.StartCommand)
					_model.StartGame(isLocalCall:false); //command.StartTime, command.StopTime);
				else if (command.IsResetCommand && command.ResetIsNewLesson)
				{
					//_model.SendStatsPacket();
					// clear the lesson string and wait until the new lesson string is received from teh server
					_viewModel.NewLesson(command.LessonText, isLocalCall:false);
				}
				else if (command.IsResetCommand && command.ResetIsRepeatedLesson)
				{
					//_model.SendStatsPacket();
					_viewModel.RepeatLesson(isLocalCall:false);
				}
			}
		}
    }
}
