using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using YamLib.Security;

namespace YamLib.Connectivity
{
	/// <summary>
	///     Connection instance
	/// </summary>
	public class HostConnection : IDisposable
	{
		private readonly ManualResetEvent _receiveDone = new ManualResetEvent(false);
		private Crypto _crypto;

		private Socket _socket;
		private Thread _thread;

		public HostConnection(Socket socket)
		{
			_socket = socket;
		}

		/// <summary>
		///     Returns whether the client is still connected
		/// </summary>
		public bool IsConnected => _socket?.Connected ?? false;

		/// <summary>
		///     Closes the connection and stops the thread
		/// </summary>
		public void Dispose()
		{
			// We dont care if anything fails... just shut it down
			try
			{
				_socket?.Shutdown(SocketShutdown.Both);
			}
			catch (Exception)
			{
			}

			try
			{
				_socket?.Close();
			}
			catch (Exception)
			{
			}

			try
			{
				_socket?.Dispose();
			}
			catch (Exception)
			{
			}

			try
			{
				_thread?.Join();
			}
			catch (Exception)
			{
			}

			try
			{
				_crypto?.Dispose();
			}
			catch (Exception)
			{
			}

			_thread = null;
			_socket = null;
			_crypto = null;
		}

		/// <summary>
		///     Gets invoked on connection established
		/// </summary>
		public event Action<HostConnection> OnClientDisconnected;

		/// <summary>
		///     Gets invoked on connection lost
		/// </summary>
		public event Action<HostConnection, byte[]> OnMessageReceived;

		public bool Authenticate()
		{
			// Create crypto instance
			_crypto = new Crypto();

			// Receive client public key
			byte[] clientKey;
			try
			{
				clientKey = ReceiveRaw();
			}
			catch (Exception ex)
			{
				throw new Exception("Could not receive remote public key: " + ex.Message);
			}

			if (clientKey == null) throw new Exception("Client sent null!");
			if (clientKey.Length <= 0) throw new Exception("Client sent empty public key");

			// Get public key
			var serverKey = _crypto.GetPublicKey();
			if (serverKey == null) throw new Exception("Could not generate public key");

			// Send public key
			try
			{
				SendRaw(serverKey);
			}
			catch (Exception ex)
			{
				throw new Exception("Could not send public key: " + ex.Message);
			}

			// Generate private key
			try
			{
				_crypto.EstablishSecretKey(clientKey);
			}
			catch (Exception ex)
			{
				throw new Exception("Couldn't generate private key: " + ex.Message);
			}

			var message = Encoding.UTF8.GetString(ReceiveEncrypted()); // TODO Fix: Possible freezing of application

			if (string.IsNullOrEmpty(message)) message = "Error";

			_ = SendEncrypted(Encoding.UTF8.GetBytes(message));

			System.Console.WriteLine("[AUTH] Received: {0}", message);

			return message == "ACK";
		}

		public void StartListening()
		{
			try
			{
				_thread = new Thread(ReceiveAsync);
				_thread.Start();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Could not start receiver handler: " + ex.Message);
			}
		}

		public byte[] ReceiveEncrypted()
		{
			byte[] encMessage;
			try
			{
				encMessage = ReceiveRaw();
			}
			catch (Exception ex)
			{
				throw new Exception("Couldn't not receive message: " + ex.Message);
			}

			var messageBytes = _crypto.Decrypt(encMessage);

			if (messageBytes == null) throw new Exception("Could not decrypt received data!");

			return messageBytes;
		}

		public bool SendEncrypted(byte[] message)
		{
			var data = _crypto.Encrypt(message);

			if (data == null) return false;

			try
			{
				SendRaw(data);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		private byte[] ReceiveRaw()
		{
			var messageLength = new byte[2];

			_ = _socket.Receive(messageLength, 0, 2, 0);

			var size = BitConverter.ToUInt16(messageLength, 0);
			var messageBytes = new byte[size];

			_ = _socket.Receive(messageBytes, 0, size, 0);

			return messageBytes;
		}

		private void SendRaw(byte[] messageBytes)
		{
			// Message should never exceed 64KiB
			if (messageBytes.Length > ushort.MaxValue)
				return;

			var messageLength = BitConverter.GetBytes((ushort) messageBytes.Length);

			var data = new byte[2 + messageBytes.Length];

			Array.Copy(messageLength, 0, data, 0, 2);
			Array.Copy(messageBytes, 0, data, 2, messageBytes.Length);

			_ = _socket.Send(data, 0, data.Length, 0);
		}

		private void ReceiveAsync()
		{
			while (IsConnected)
			{
				_ = _receiveDone.Reset();
				try
				{
					var state = new StateObject(2);

					_ = _socket.BeginReceive(state.bytes, 0, state.length, 0, MessageLengthReceivedCallback, state);
				}
				catch (SocketException)
				{
					System.Console.WriteLine("Connection lost!");
					_ = _receiveDone.Set();
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("Client error: {0}", ex.Message);
					_ = _receiveDone.Set();
				}

				_ = _receiveDone.WaitOne();
			}

			OnClientDisconnected?.Invoke(this); // NOTE Crashes if null
		}

		private void MessageLengthReceivedCallback(IAsyncResult asyncResult)
		{
			try
			{
				var state = (StateObject) asyncResult.AsyncState;
				var bytesRead = 0;

				bytesRead = _socket.EndReceive(asyncResult);

				if (bytesRead == 0)
				{
					_socket.Close();
					_ = _receiveDone.Set();
					return;
				}

				if (bytesRead == state.length)
				{
					var size = BitConverter.ToUInt16(state.bytes, 0);

					state.length = size;
					state.bytes = new byte[size];

					_ = _socket.BeginReceive(state.bytes, 0, state.length, 0, MessageReceivedCallback, state);
				}
				else
				{
					_ = _receiveDone.Set();
				}
			}
			catch (SocketException)
			{
				_ = _receiveDone.Set();
				System.Console.WriteLine("Connection lost!");
			}
			catch (Exception ex)
			{
				_ = _receiveDone.Set();
				System.Console.WriteLine("Client error: {0}", ex.Message);
			}
		}

		private void MessageReceivedCallback(IAsyncResult asyncResult)
		{
			try
			{
				var state = (StateObject) asyncResult.AsyncState;

				var bytesRead = 0;

				bytesRead = _socket.EndReceive(asyncResult);

				if (bytesRead == 0)
				{
					_socket.Close();
					_receiveDone.Set();
					return;
				}

				if (bytesRead == state.length)
				{
					var messageBytes = _crypto.Decrypt(state.bytes);

					if (messageBytes == null) throw new Exception("Could not decrypt message");

					_ = _receiveDone.Set();
					OnMessageReceived.Invoke(this, messageBytes);
				}
			}
			catch (SocketException)
			{
				_ = _receiveDone.Set();
				System.Console.WriteLine("Connection lost!");
			}
			catch (Exception ex)
			{
				_ = _receiveDone.Set();
				System.Console.WriteLine("Client error: {0}", ex.Message);
			}
		}

		public string Address()
		{
			var ep = _socket.RemoteEndPoint as IPEndPoint;

			return $"{ep.Address}:{ep.Port}";
		}

		private class StateObject
		{
			public byte[] bytes;
			public ushort length;

			public StateObject(ushort length)
			{
				this.length = length;
				bytes = new byte[length];
			}
		}
	}
}