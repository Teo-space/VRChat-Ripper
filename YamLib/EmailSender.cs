using System;
using System.Net;
using System.Net.Mail;

namespace YamLib
{
	public static class EmailTools
	{
		/// <summary>
		///     Returns if the input string is a valid email format
		/// </summary>
		/// <param name="email">The email to be evaluated</param>
		/// <returns>
		///     Is format valid?
		/// </returns>
		public static bool IsValidEmail(string email)
		{
			if (!string.IsNullOrWhiteSpace(email))
				try
				{
					var addr = new MailAddress(email);
					return addr.Address == email;
				}
				catch (FormatException)
				{
				}

			return false;
		}

		public class EmailClient
		{
			private readonly NetworkCredential credential;
			private readonly MailAddress mailAddress;
			private readonly SmtpClient smtpClient;

			public EmailClient(string host, int port, string email, string password)
			{
				smtpClient = new SmtpClient(host);
				mailAddress = new MailAddress(email);
				credential = new NetworkCredential(email, password);

				smtpClient.Port = port;
				smtpClient.Credentials = credential;
				smtpClient.EnableSsl = true;
			}

			public bool SendEmail(string[] recepients, string subject, string body)
			{
				if (recepients == null || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(subject))
					return false;

				var mail = new MailMessage();

				mail.From = mailAddress;
				foreach (var recepient in recepients)
					if (IsValidEmail(recepient))
						mail.Bcc.Add(recepient);
				mail.Subject = subject;
				mail.Body = body;

				smtpClient.Send(mail);
				return true;
			}
		}
	}
}