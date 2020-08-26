using System;

namespace VRC
{
	[Serializable]
	public struct VrcId : IComparable, IComparable<Guid>, IComparable<VrcId>, IEquatable<Guid>, IEquatable<VrcId>
	{
		private Guid guid;

		public enum VrcIdType
		{
			None,
			Offline,
			Not,
			LegacyUser,
			User,
			Avatar,
			World,
			LegacyWorld,
			AuthCookie,
			UnityPackage,
			PlayerModeration,
			File
		}

		public VrcIdType IdType { get; private set; }

		public Guid Guid => guid;

		public static readonly VrcId Empty;

		public VrcId(string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			if (str == "none")
			{
				guid = Guid.Empty;
				IdType = VrcIdType.None;
				return;
			}

			if (str == "offline")
			{
				guid = Guid.Empty;
				IdType = VrcIdType.Offline;
				return;
			}

			string[] elements = str.Split('_');
			if (elements.Length != 2)
			{
				if (str.Length != 10)
					throw new ArgumentException("Input guid must be a valid Guid or 10 a char b64 string!");
				IdType = VrcIdType.LegacyUser;
				guid = new Guid(Convert.FromBase64String($"AAAAAAAAAAAA{str}=="));
				return;
			}

			switch (elements[0])
			{
				case "avtr": // AvatarId
					IdType = VrcIdType.Avatar;
					break;
				case "usr": // UserId
					IdType = VrcIdType.User;
					break;
				case "wrld": // Post-2018 WorldId prefix
					IdType = VrcIdType.World;
					break;
				case "wld": // Pre-2018 WorldId prefix
					IdType = VrcIdType.LegacyWorld;
					break;
				case "unp": // UnityPackageId
					IdType = VrcIdType.UnityPackage;
					break;
				case "pmod": // Player moderation
					IdType = VrcIdType.PlayerModeration;
					break;
				case "not": // Notification
					IdType = VrcIdType.Not;
					break;
				case "authcookie": // Authcookie
					IdType = VrcIdType.AuthCookie;
					break;
				case "file": // File
					IdType = VrcIdType.AuthCookie;
					break;
				default:
					throw new ArgumentException("VrcId prefix is invalid!");
			}

			guid = new Guid(elements[1]);
		}
		public VrcId(VrcIdType type, Guid id)
		{
			IdType = type;
			guid = id;
		}
		public VrcId(VrcIdType type, string id)
		{
			IdType = type;
			guid = new Guid(id);
		}
		public static VrcId NewVrcId(VrcIdType t)
		{
			return new VrcId
			{
				IdType = t,
				guid = Guid.NewGuid()
			};
		}

		public int CompareTo(VrcId value)
		{
			return guid.CompareTo(value.guid) + (value.IdType - IdType);
		}

		public int CompareTo(Guid value)
		{
			return guid.CompareTo(value);
		}

		public int CompareTo(object o)
		{
			if (o is VrcId)
				return CompareTo((VrcId) o);
			if (o is Guid)
				return CompareTo((Guid) o);
			throw new ArgumentException("Value is not a Guid or VrcId");
		}

		public bool Equals(VrcId g)
		{
			return guid.Equals(g.guid) && IdType == g.IdType;
		}

		public bool Equals(Guid g)
		{
			return guid.Equals(g);
		}

		public override bool Equals(object o)
		{
			if (o is VrcId)
				return CompareTo((VrcId) o) == 0;
			if (o is Guid)
				return CompareTo((Guid) o) == 0;
			return false;
		}

		public override int GetHashCode()
		{
			return guid.GetHashCode() + (int) IdType;
		}

		public override string ToString()
		{
			switch (IdType)
			{
				case VrcIdType.User:
					return $"usr_{guid:D}";
				case VrcIdType.Avatar:
					return $"avtr_{guid:D}";
				case VrcIdType.World:
					return $"wrld_{guid:D}";
				case VrcIdType.LegacyWorld:
					return $"wld_{guid:D}";
				case VrcIdType.PlayerModeration:
					return $"pmod_{guid:D}";
				case VrcIdType.UnityPackage:
					return $"unp_{guid:D}";
				case VrcIdType.Not:
					return $"not_{guid:D}";
				case VrcIdType.AuthCookie:
					return $"authcookie_{guid:D}";
				case VrcIdType.File:
					return $"file_{guid:D}";
				case VrcIdType.LegacyUser:
					return Convert.ToBase64String(guid.ToByteArray()).Substring(12, 10);
				case VrcIdType.Offline:
					return "offline";
				default:
					return "none";
			}
		}

		public static bool operator ==(VrcId a, VrcId b)
		{
			return a.guid == b.guid && a.IdType == b.IdType;
		}

		public static bool operator !=(VrcId a, VrcId b)
		{
			return a.guid != b.guid || a.IdType != b.IdType;
		}

		public static bool operator ==(VrcId a, Guid b)
		{
			return a.guid == b;
		}

		public static bool operator !=(VrcId a, Guid b)
		{
			return a.guid != b;
		}
	}
}