using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssetDB
{
	public class SessionProtector
	{
		private object l = new object();
		private bool m_appIsExiting = false;
		private UInt64 m_sessions = 0;

		public void Exit(bool blockingCall = true)
		{
			lock (l)
			{
				m_appIsExiting = true;
			}
			if (blockingCall)
			{
				while (ProcCount() > 0)
				{
					Thread.Sleep(100);
				}
			}
		}
		public void ExitAfterProcs(int msBetweenChecks = 1000)
		{
			while (ProcCount() > 0)
			{
				Thread.Sleep(msBetweenChecks);
			}
			lock (l)
			{
				m_appIsExiting = true;
			}
		}

		public bool IsExiting()
		{
			lock (l)
			{
				return m_appIsExiting;
			}
		}

		public UInt64 ProcCount()
		{
			lock (l)
			{
				return m_sessions;
			}
		}

		public bool TryStartSession()
		{
			lock (l)
			{
				if (!m_appIsExiting)
				{
					m_sessions++;
					return true;
				}
			}
			return false;
		}

		public void EndSession()
		{
			lock (l)
			{
				m_sessions--;
			}
		}
	}
}
