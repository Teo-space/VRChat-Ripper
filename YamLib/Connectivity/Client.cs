using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamLib.Security;

namespace YamLib.Connectivity
{
	/// <summary>
	///     Client instance
	/// </summary>
	public class Client : IDisposable
	{
		private Crypto _crypto;

		private Socket _socket;
		private Thread _thread;

		/// <summary>
		///     Returns if the socket is still connected
		/// </summary>
		public bool IsConnected => _socket?.Connected ?? false;

		public void Dispose()
		{
			Cleanup();
		}

		/// <summary>
		///     Gets invoked on client connect
		/// </summary>
		public event Action<Client> OnConnected;

		/// <summary>
		///     Gets invoked on client disconnect
		/// </summary>
		public event Action<Client> OnDisconnected;

		/// <summary>
		///     Gets invoked when client sends a message
		/// </summary>
		public event Action<Client, byte[]> OnMessageReceived;

		/// <summary>
		///     Closes the connection and stops the thread
		/// </summary>
		public void Cleanup()
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
				_thread?.Abort();
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
		///     Connects to the specified uri and port
		/// </summary>
		/// <returns>
		///     Return <see langword="true" /> if connection was successfull
		/// </returns>
		/// <param name="hostUri"></param>
		/// <param name="port"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="SocketException"></exception>
		/// <exception cref="System.Security.SecurityException"></exception>
		public bool Connect(string hostUri, ushort port)
		{
			Cleanup();

			if (hostUri == null) throw new ArgumentNullException("HostURI is null.");

			IPAddress[] addresses;

			// Establish the local endpoint for the socket.
			try
			{
				addresses = Dns.GetHostEntry(hostUri).AddressList;
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new ArgumentOutOfRangeException("hostUri is more than 255 characters long.");
			}
			catch (SocketException ex)
			{
				System.Console.WriteLine("An error was encountered when resolving the hostUri: {0}", ex.Message);
				return false;
			}
			catch (ArgumentException)
			{
				System.Console.WriteLine("hostUri is an invalid IP address");
				return false;
			}

			if (addresses.Length == 0 || addresses[0] == null) return false;

			var remoteEndPoint = new IPEndPoint(addresses[0], port);

			// Create a TCP/IP socket
			_socket = new Socket(addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Bind the socket to the local endpoint and listen for incoming connections
			_socket.Connect(remoteEndPoint);

			OnConnected?.Invoke(this);
			return true;
		}

		/// <summary>
		///     Perfroms ECDH key-exhange
		/// </summary>
		/// <returns>
		///     Returns true if authentication succeeded
		/// </returns>
		public bool Authenticate()
		{
			// Create crypto instance
			_crypto = new Crypto();

			// Get public key
			var clientKey = _crypto.GetPublicKey();

			// Send public key
			try
			{
				SendRaw(clientKey);
			}
			catch (SocketException ex)
			{
				System.Console.WriteLine("SocketException Caught!");
				System.Console.WriteLine("Could not send public key: " + ex.Message);
				_crypto = null;
				return false;
			}

			// Receive server public key
			byte[] serverKey;
			try
			{
				serverKey = ReceiveRaw();
			}
			catch (SecurityException ex)
			{
				System.Console.WriteLine("SecurityException Caught!");
				System.Console.WriteLine("Could not send public key: " + ex.Message);
				_crypto = null;
				return false;
			}
			catch (SocketException ex)
			{
				System.Console.WriteLine("SocketException Caught!");
				System.Console.WriteLine("Could not send public key: " + ex.Message);
				_crypto = null;
				return false;
			}

			if (serverKey == null)
			{
				System.Console.WriteLine("Server sent null!");
				_crypto = null;
				return false;
			}

			// Generate private key
			try
			{
				_crypto.EstablishSecretKey(serverKey);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Couldn't generate private key: " + ex.Message);
				_crypto = null;
				return false;
			}

			SendEncrypted(Encoding.UTF8.GetBytes("ACK"));

			return Encoding.UTF8.GetString(ReceiveEncrypted()) == "ACK";
		}

		/// <summary>
		///     Starts message listener thread
		/// </summary>
		/// <exception cref="ThreadStateException"></exception>
		/// <exception cref="OutOfMemoryException"></exception>
		public void StartListening()
		{
			_thread = new Thread(ReceiveAsync);
			_thread.Start();
		}

		/// <summary>
		///     Reads a incoming message from client (blocking call)
		/// </summary>
		/// <returns>
		///     Returns the read message, or null if it could not be decrypted
		/// </returns>
		public byte[] ReceiveEncrypted()
		{
			if (_crypto == null) // DEBUG
			{
				System.Console.WriteLine("Cannot receive message, please authenticate first"); // DEBUG
				return null; // DEBUG
			}

			byte[] encMessage;
			try
			{
				encMessage = ReceiveRaw();
			}
			catch (Exception)
			{
				return null; // TODO should maybe do something else? (catch when ReceiveBytes() is finished)
			}

			byte[] messageBytes;
			try
			{
				messageBytes = _crypto.Decrypt(encMessage);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("ReceiveMessage -> splitting: " + ex.Message); // DEBUG
				return null;
			}

			return messageBytes;
		}

		/// <summary>
		///     Sends message to client
		/// </summary>
		/// <param name="message">
		///     Message to send to client
		/// </param>
		public void SendEncrypted(byte[] message)
		{
			if (_crypto == null) // DEBUG
			{
				System.Console.WriteLine("Cannot send message, please authenticate first"); // DEBUG
				return; // DEBUG
			}

			byte[] data;
			try
			{
				data = _crypto?.Encrypt(message);
			}
			catch (EncoderFallbackException ex)
			{
				System.Console.WriteLine("Could not get bytes: " + ex.Message); // DEBUG
				System.Console.WriteLine(ex.HelpLink); // DEBUG
				return;
			}

			if (data == null)
			{
				System.Console.WriteLine("Encryption failed!"); // DEBUG
				return;
			}

			try
			{
				SendRaw(data);
			}
			catch (SocketException ex)
			{
				System.Console.WriteLine(ex.HelpLink); // DEBUG
				System.Console.WriteLine("Could not send message: " + ex.Message); // DEBUG
			}
		}

		/// <summary>
		///     Recieves bytes from the client (blocking call)
		/// </summary>
		/// <returns>
		///     Returns bytes read, or null if it failed
		/// </returns>
		/// <exception cref="SocketException"></exception>
		/// <exception cref="SecurityException"></exception>
		private byte[] ReceiveRaw()
		{
			var data = new byte[2];

			try
			{
				_ = _socket.Receive(data, 0, 2, 0);
			}
			catch (ObjectDisposedException)
			{
				return null;
			}

			var size = BitConverter.ToUInt16(data, 0);
			data = new byte[size];

			try
			{
				_ = _socket.Receive(data, 0, size, 0);
			}
			catch (ObjectDisposedException)
			{
				return null;
			}

			return data;
		}

		/// <summary>
		///     Sends bytes to the client
		/// </summary>
		/// <param name="messageBytes"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="SocketException"></exception>
		private void SendRaw(byte[] messageBytes)
		{
			if (messageBytes == null) throw new ArgumentNullException();

			if (messageBytes.Length + 2 > ushort.MaxValue)
				throw new ArgumentOutOfRangeException("message length can not exceed 65535 bytes!");

			if (messageBytes.Length == 0) return;

			// Get message length to use as a header
			var messageLength = BitConverter.GetBytes((ushort) messageBytes.Length);

			var data = new byte[2 + messageBytes.Length];

			// Combine the arrays
			Array.Copy(messageLength, 0, data, 0, 2);
			Array.Copy(messageBytes, 0, data, 2, messageBytes.Length);

			try
			{
				_ = _socket.BeginSend(data, 0, data.Length, 0, SendBytesCallback, null);
			}
			catch (ObjectDisposedException)
			{
			}
		}

		private void SendBytesCallback(IAsyncResult asyncResult)
		{
			_ = _socket.EndSend(asyncResult);
		}

		/// <summary>
		///     Listens to incoming messages, and starts async receivers (blocking call)
		/// </summary>
		private void ReceiveAsync()
		{
			using (var receiveDone = new ManualResetEvent(false))
			{
				while (IsConnected)
				{
					_ = receiveDone.Reset();
					var state = new StateObject
					{
						length = 2,
						bytes = new byte[2],
						signal = receiveDone
					};

					try
					{
						_ = _socket.BeginReceive(state.bytes, 0, state.length, 0, MessageLengthReceivedCallback, state);
					}
					catch (Exception ex)
					{
						System.Console.WriteLine("Exception caught: {0}", ex.Message);
					}

					_ = receiveDone.WaitOne();
				}
			}

			OnDisconnected.Invoke(this);
		}

		private void MessageLengthReceivedCallback(IAsyncResult asyncResult)
		{
			var state = (StateObject) asyncResult.AsyncState;
			var bytesRead = 0;

			try
			{
				bytesRead = _socket.EndReceive(asyncResult);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Could not receive client message: " + ex.Message);
				try
				{
					_ = state.signal.Set();
				}
				catch (Exception)
				{
				}

				return;
			}

			if (bytesRead == state.length)
			{
				state.length = BitConverter.ToUInt16(state.bytes, 0);

				state.bytes = new byte[state.length];

				try
				{
					_ = _socket.BeginReceive(state.bytes, 0, state.length, 0, MessageReceivedCallback, state);
					return;
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("Could not receive client message: " + ex.Message);
				}
			}

			try
			{
				_ = state.signal.Set();
			}
			catch (Exception)
			{
			}
		}

		private void MessageReceivedCallback(IAsyncResult asyncResult)
		{
			var state = (StateObject) asyncResult.AsyncState;
			var bytesRead = 0;

			try
			{
				bytesRead = _socket.EndReceive(asyncResult);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Could not receive client message: " + ex.Message);
				try
				{
					_ = state.signal.Set();
				}
				catch (Exception)
				{
				}

				return;
			}

			if (bytesRead == state.length)
			{
				var encMessage = state.bytes;
				byte[] messageBytes = null;

				try
				{
					messageBytes = _crypto.Decrypt(encMessage);
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("Received message format is invalid: " + ex.Message);
					try
					{
						_ = state.signal.Set();
					}
					catch (Exception)
					{
					}

					return;
				}

				try
				{
					_ = state.signal.Set();
				}
				catch (Exception)
				{
				}

				_ = Task.Run(() => OnMessageReceived.Invoke(this, messageBytes));
				return;
			}

			System.Console.WriteLine("Message length mismatch!");
			try
			{
				_ = state.signal.Set();
			}
			catch (Exception)
			{
			}
		}

		private class StateObject
		{
			public byte[] bytes;
			public ushort length;
			public ManualResetEvent signal;
		}
	}
}