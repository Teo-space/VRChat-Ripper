// Special libs

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using VRC.API;

namespace SimpRipper
{
	public partial class SimpRipper : Form
	{
		private ApiAvatar _avatar;

		public SimpRipper()
		{
			InitializeComponent();
		}

		private void AuthorLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (_avatar == null) return;
			var url = "https://www.vrchat.net/home/user/" + _avatar.authorId;
			_ = Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
		}

		private bool LoadPage()
		{
			var html = string.Empty;
			var url = "https://www.vrchat.net/api/1/avatars/" + avatarIDBox.Text + "?apiKey=" + apiKeyBox.Text;
			try
			{
				var request = (HttpWebRequest) WebRequest.Create(url);
				request.AutomaticDecompression = DecompressionMethods.None;

				using (var response = (HttpWebResponse) request.GetResponse())
				using (var stream = response.GetResponseStream())
				using (var reader = new StreamReader(stream))
				{
					html = reader.ReadToEnd();
				}

				if (html.Contains("Avatar not found"))
				{
					_avatar = null;
					avatarIDBox.BackColor = Color.Red;
					return false;
				}

				avatarIDBox.BackColor = Color.Green;
				if (html.Contains("message\":\"\"API Key incorrect!\"\", \"status_code\":403"))
				{
					_avatar = null;
					apiKeyBox.BackColor = Color.Red;
					return false;
				}

				apiKeyBox.BackColor = Color.Green;

				_avatar = JsonConvert.DeserializeObject<ApiAvatar>(html);
			}
			catch
			{
				_avatar = null;
				avatarIDBox.BackColor = Color.Red;
				apiKeyBox.BackColor = Color.Red;
				return false;
			}

			if (_avatar == null)
			{
				avatarIDBox.BackColor = Color.Red;
				apiKeyBox.BackColor = Color.Red;
				return false;
			}

			avatarName.Text = _avatar.name;
			avatarID.Text = _avatar.id.ToString();
			avatarDescription.Text = _avatar.description;
			avatarReleaseStatus.Text = _avatar.releaseStatus;
			authorID.Text = _avatar.authorId.ToString();
			authorName.Text = _avatar.authorName;
			avatarIDBox.BackColor = Color.Green;
			apiKeyBox.BackColor = Color.Green;
			return true;
		}

		private void DownloadAssetFile()
		{
			if (_avatar == null) return;

			if (!Directory.Exists("assetCache"))
				_ = Directory.CreateDirectory("assetCache");

			WebClient client = new RedirectClient();

			var path = "assetCache/" + _avatar.id + ".vrca";

			client.DownloadFile(_avatar.assetUrl, path);
			client.Dispose();
		}

		private void AvatarIDBox_TextChanged(object sender, EventArgs e)
		{
			apiKeyBox.Enabled = false;
			avatarIDBox.Enabled = false;
			if (avatarIDBox.Text == null || avatarIDBox.Text == "")
			{
				apiKeyBox.Enabled = true;
				avatarIDBox.Enabled = true;
				avatarIDBox.BackColor = Color.Red;
				return;
			}

			avatarIDBox.BackColor = Color.Green;
			if (apiKeyBox.Text == null || apiKeyBox.Text == "")
			{
				apiKeyBox.Enabled = true;
				avatarIDBox.Enabled = true;
				apiKeyBox.BackColor = Color.Red;
				return;
			}

			apiKeyBox.BackColor = Color.Green;

			if (!LoadPage())
			{
				apiKeyBox.Enabled = true;
				avatarIDBox.Enabled = true;
				return;
			}

			avatarImageBox.ImageLocation = _avatar.thumbnailImageUrl;

			_ = Task.Run(() => DownloadAssetFile());

			apiKeyBox.Enabled = true;
			avatarIDBox.Enabled = true;
		}
	}

	public class RedirectClient : WebClient
	{
		protected override WebResponse GetWebResponse(WebRequest request)
		{
			(request as HttpWebRequest).AllowAutoRedirect = true;
			var response = base.GetWebResponse(request);
			return response;
		}
	}
}