using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using YamLib;

namespace VRC
{
	[System.ComponentModel.DesignerCategory("Code")] // Fixes this class item showing up as a forms item
	internal class JsonWebClient : WebClient
	{
		private bool m_discardCookies = false;
		private static long procCounter = 0;

		public bool DiscardCookies
		{
			get { return m_discardCookies;  }
			set { m_discardCookies = value; }
		}


		public static long RequestCount()
		{
			return Interlocked.Read(ref procCounter);
		}

		private readonly JsonSerializerSettings settings = new JsonSerializerSettings
		{
			MissingMemberHandling = MissingMemberHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		public CookieContainer cookies = new CookieContainer();

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);

			if (m_discardCookies)
			{
				request.Headers.Clear();
			}
			else
			{
				if (request is HttpWebRequest)
					((HttpWebRequest)request).CookieContainer = cookies;
			}

			return request;
		}

		public void DownloadJsonAsync<T>(Uri uri, Action<object> onDownloadComplete, Action<Exception> onError)
		{
			Interlocked.Increment(ref procCounter);

			DownloadStringCompletedEventHandler handler = null;

			handler = (_, e) =>
			{
				DownloadStringCompleted -= handler;

				if (e.Error != null)
				{
					onError.Invoke(e.Error);
					Interlocked.Decrement(ref procCounter);
					return;
				}

				string result = e.Result;
				bool typeIsArray = typeof(T).IsArray;
				bool jsonIsArray = result[0] == '[';

				object obj = null;
				try
				{
					// Make json object an array of 1 if the caller expects an array
					if (typeIsArray && !jsonIsArray)
					{
						result = $"[{result}]";
						jsonIsArray = true;
					}

					// If json returned was an array when caller expected an object, return the first element if array is size of array is 1, else dont return anything
					// Else return the expected type
					if (!typeIsArray && jsonIsArray)
						obj = ToolBox.ArrToObj(JsonConvert.DeserializeObject<T[]>(result, settings));
					else
						obj = JsonConvert.DeserializeObject<T>(result, settings);
				}
				catch (Exception ex)
				{
					onError.Invoke(ex);
					Interlocked.Decrement(ref procCounter);
					return;
				}

				onDownloadComplete?.Invoke(obj);
				Interlocked.Decrement(ref procCounter);
			};

			DownloadStringCompleted += handler;
			DownloadStringAsync(uri);
		}
	}
}
