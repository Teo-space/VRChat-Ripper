using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security;

namespace YamLib.Connectivity
{
	/// <summary>
	///     Listen for incoming connections, and create a new <c>Connection</c> object when when a client connects
	/// </summary>
	public class UdpConnection : IDisposable
	{
		private readonly IPAddress anyIP;
		private readonly AddressFamily family;

		private readonly byte[] buffer = new byte[1024];

		private readonly List<EndPoint> clientList = new List<EndPoint>();

		private Socket socket;

		public UdpConnection(bool useIPv6)
		{
			if (useIPv6)
			{
				anyIP = IPAddress.IPv6Any;
				family = AddressFamily.InterNetworkV6;
			}
			else
			{
				anyIP = IPAddress.Any;
				family = AddressFamily.InterNetwork;
			}
		}

		public void Dispose()
		{
			try
			{
				StopListening();
			}
			catch (Exception)
			{
			}

			try
			{
				clientList.Clear();
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     Gets invoked when a client connects.
		///     You need to start listening for this event to fire.
		/// </summary>
		public event Action<EndPoint, byte[]> OnMessageReceived;

		/// <summary>
		///     Starts listening for connecting clients, will invoke <c>OnClientConnected</c> when a client connects. (blocking
		///     call)
		/// </summary>
		/// <param name="port"></param>
		/// <exception cref="SocketException"></exception>
		/// <exception cref="SecurityException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public void Listen(int port)
		{
			socket = new Socket(
				family,
				SocketType.Dgram,
				ProtocolType.Udp
			);

			socket.SetSocketOption(
				SocketOptionLevel.Socket,
				SocketOptionName.ReuseAddress,
				true
			);

			socket.Bind(new IPEndPoint(
				anyIP,
				port
			));

			StartListening();
		}

		private void StartListening()
		{
			EndPoint newClientEP = new IPEndPoint(anyIP, 0);
			socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref newClientEP, HandleMessage,
				newClientEP);
		}

		private void HandleMessage(IAsyncResult iar)
		{
			try
			{
				EndPoint clientEP = (IPEndPoint) iar.AsyncState; //new IPEndPoint(anyIP, 0);
				var dataLen = socket.EndReceiveFrom(iar, ref clientEP);

				if (!clientList.Exists(client => client.Equals(clientEP)))
					clientList.Add(clientEP);

				var data = new byte[dataLen];
				Array.Copy(buffer, data, dataLen);

				StartListening();

				OnMessageReceived.Invoke(clientEP, data);
			}
			catch (Exception)
			{
			}
		}

		public void SendTo(byte[] data, EndPoint clientEP)
		{
			try
			{
				socket.SendTo(data, clientEP);
			}
			catch (SocketException)
			{
				clientList.Remove(clientEP);
			}
		}

		public void SendToAll(byte[] data)
		{
			for (var i = 0; i < clientList.Count; ++i)
				try
				{
					socket.SendTo(data, clientList[i]);
				}
				catch (SocketException)
				{
					clientList.Remove(clientList[i]);
					--i;
				}
		}

		/// <summary>
		///     Stops the listener, and unblocks the thread that called it
		/// </summary>
		public void StopListening()
		{
			socket?.Close();
			socket = null;
		}
	}
}