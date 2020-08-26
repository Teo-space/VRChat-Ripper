using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VRC.API;

namespace VRC
{
	public class ApiClient : IDisposable
	{
		// VRChat constants
		private const string s_apiUrl = "https://www.vrchat.net/api/1/";
		private const string s_apiOrganization = "vrchat";

		private static string m_apiKeyQuery = "?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26";

		private bool m_isDisposing = false;
		private object l_isDisposing = new object();
		private bool IsDisposing
		{
			get
			{
				lock (l_isDisposing)
					return m_isDisposing;
			}
			set
			{
				lock (l_isDisposing)
					m_isDisposing = value;
			}
		}

		// WebClient pool
		private readonly object l_clients = new object();
		private List<JsonWebClient> m_clients = new List<JsonWebClient>();

		public bool IsOpen => m_clients != null;

		// Threadsafe WebClient request pooling
		private void DownloadJsonConcurrent<T>(Uri uri, Action<object> onDownloadComplete, Action<Exception> onError)
		{
			lock (l_clients)
			{
				if (m_clients == null)
					return;

				foreach (var client in m_clients)
				{
					if (!client.IsBusy)
					{
						client.DownloadJsonAsync<T>(uri, onDownloadComplete, onError);
						return;
					}
				}

				var newClient = new JsonWebClient();
				newClient.Encoding = Encoding.UTF8;
				newClient.DiscardCookies = true; // Bypass security
				m_clients.Add(newClient);
				newClient.DownloadJsonAsync<T>(uri, onDownloadComplete, onError);
			}
		}

		public void AbortRequests(bool disableClient)
		{
			lock (l_clients)
			{
				if (m_clients == null)
					return;

				for (int i = 0; i < m_clients.Count; i++)
				{
					m_clients[i].CancelAsync();
					m_clients[i].Dispose();
				}
				if (disableClient)
					m_clients = null;
			}
		}

		public void CloseRequets(bool disableClient)
		{
			lock (l_clients)
			{
				if (m_clients == null)
					return;

				while (JsonWebClient.RequestCount() != 0)
					Thread.Sleep(10);

				for (int i = 0; i < m_clients.Count; i++)
				{
					m_clients[i].CancelAsync();
					m_clients[i].Dispose();
				}

				if (disableClient)
					m_clients = null;
			}
		}

		// Public Methods
		public void GetAvatarById(VrcId avatarId, Action<ApiAvatar> onSuccess, Action<VrcId, Exception> onFailure)
		{
			if (avatarId.IdType != VrcId.VrcIdType.Avatar)
				return;

			DownloadJsonConcurrent<ApiAvatar>(new Uri($"{s_apiUrl}avatars/{avatarId}{m_apiKeyQuery}"),
				obj => onSuccess.Invoke((ApiAvatar)obj),
				ex => onFailure.Invoke(avatarId, ex));
		}

		public void GetWorldById(VrcId worldId, Action<ApiWorld> onSuccess, Action<VrcId, Exception> onFailure)
		{
			if (worldId.IdType != VrcId.VrcIdType.World)
				return;

			DownloadJsonConcurrent<ApiWorld>(new Uri($"{s_apiUrl}worlds/{worldId}{m_apiKeyQuery}"),
				obj => onSuccess.Invoke((ApiWorld)obj),
				ex => onFailure.Invoke(worldId, ex));
		}

		public void GetWorldInstanceById(VrcId worldId, string InstanceId, Action<ApiWorld> onSuccess, Action<VrcId, string, Exception> onFailure)
		{
			if (worldId.IdType != VrcId.VrcIdType.World)
				return;

			DownloadJsonConcurrent<ApiWorld>(new Uri($"{s_apiUrl}worlds/{worldId}/{InstanceId}{m_apiKeyQuery}"),
				obj => onSuccess.Invoke((ApiWorld)obj),
				ex => onFailure.Invoke(worldId, InstanceId, ex));
		}

		public void GetUserById(VrcId userId, Action<ApiUser> onSuccess, Action<VrcId, Exception> onFailure)
		{
			if (userId.IdType != VrcId.VrcIdType.User)
				return;

			DownloadJsonConcurrent<ApiUser>(new Uri($"{s_apiUrl}users/{userId}{m_apiKeyQuery}"),
				obj => onSuccess.Invoke((ApiUser)obj),
				ex => onFailure.Invoke(userId, ex));
		}

		public void Dispose()
		{
			IsDisposing = true;
			AbortRequests(true);
		}
	}
}
