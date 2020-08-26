using System;

namespace VRC.API
{
	[Serializable]
	public struct ApiUserTag : IComparable, IComparable<ApiUserTag>, IEquatable<ApiUserTag>,
		IComparable<ApiUserTag.Enum>, IEquatable<ApiUserTag.Enum>
	{
		public enum Enum
		{
			// Ranks
			admin_moderator, // Moderator
			system_legend, // Probably spent alot of time ingame
			system_trust_legend, // Veteran, Probably spent alot of time ingame
			system_trust_veteran, // Trusted
			system_trust_intermediate, // Intermediate trust level
			system_trust_trusted, // Known
			system_trust_known, // User
			system_trust_basic, // New User
			system_probable_troll, // Probably been reported multiple times and is (probably) a troll
			system_troll, // Probably confirmed troll.

			// Can send feedback
			system_feedback_access,

			// can publish avatars
			system_avatar_access,
			admin_avatar_access, // probably the same as system_avatar_access

			// can publish worlds
			system_world_access,
			admin_world_access, // probably the same as system_world_access

			// Has scripting access (can upload user made scripts)
			system_scripting_access,
			admin_scripting_access, // probably the same as system_scripting_access

			// Replace avatarimage with vrchat logo
			admin_official_thumbnail,

			// Unknown
			show_social_rank,
			admin_avatar_restricted,
			admin_world_restricted,
			admin_lock_level,
			admin_lock_tags,

			// Languages
			language_eng,
			language_kor,
			language_rus,
			language_spa,
			language_por,
			language_zho,
			language_deu,
			language_jpn,
			language_fra,
			language_swe,
			language_nld,
			language_pol,
			language_dan,
			language_nor,
			language_ita,
			language_tha,
			language_fin,
			language_hun,
			language_ces,
			language_tur,
			language_ara
		}

		public Enum Tag { get; set; }

		public ApiUserTag(string str)
		{
			switch (str)
			{
				case "admin_moderator":
					Tag = Enum.admin_moderator;
					break;
				case "system_trust_legend":
					Tag = Enum.system_trust_legend;
					break;
				case "system_trust_veteran":
					Tag = Enum.system_trust_veteran;
					break;
				case "system_trust_intermediate":
					Tag = Enum.system_trust_intermediate;
					break;
				case "system_trust_trusted":
					Tag = Enum.system_trust_trusted;
					break;
				case "system_trust_known":
					Tag = Enum.system_trust_known;
					break;
				case "system_trust_basic":
					Tag = Enum.system_trust_basic;
					break;
				case "system_probable_troll":
					Tag = Enum.system_probable_troll;
					break;
				case "system_troll":
					Tag = Enum.system_troll;
					break;
				case "system_feedback_access":
					Tag = Enum.system_feedback_access;
					break;
				case "system_scripting_access":
					Tag = Enum.system_scripting_access;
					break;
				case "system_avatar_access":
					Tag = Enum.system_avatar_access;
					break;
				case "system_world_access":
					Tag = Enum.system_world_access;
					break;
				case "admin_scripting_access":
					Tag = Enum.admin_scripting_access;
					break;
				case "admin_avatar_access":
					Tag = Enum.admin_avatar_access;
					break;
				case "admin_world_access":
					Tag = Enum.admin_world_access;
					break;
				case "show_social_rank":
					Tag = Enum.show_social_rank;
					break;
				case "admin_avatar_restricted":
					Tag = Enum.admin_avatar_restricted;
					break;
				case "admin_world_restricted":
					Tag = Enum.admin_world_restricted;
					break;
				case "admin_lock_level":
					Tag = Enum.admin_lock_level;
					break;
				case "admin_lock_tags":
					Tag = Enum.admin_lock_tags;
					break;
				case "admin_official_thumbnail":
					Tag = Enum.admin_official_thumbnail;
					break;
				case "language_eng":
					Tag = Enum.language_eng;
					break;
				case "language_kor":
					Tag = Enum.language_kor;
					break;
				case "language_rus":
					Tag = Enum.language_rus;
					break;
				case "language_spa":
					Tag = Enum.language_spa;
					break;
				case "language_por":
					Tag = Enum.language_por;
					break;
				case "language_zho":
					Tag = Enum.language_zho;
					break;
				case "language_deu":
					Tag = Enum.language_deu;
					break;
				case "language_jpn":
					Tag = Enum.language_jpn;
					break;
				case "language_fra":
					Tag = Enum.language_fra;
					break;
				case "language_swe":
					Tag = Enum.language_swe;
					break;
				case "language_nld":
					Tag = Enum.language_nld;
					break;
				case "language_pol":
					Tag = Enum.language_pol;
					break;
				case "language_dan":
					Tag = Enum.language_dan;
					break;
				case "language_nor":
					Tag = Enum.language_nor;
					break;
				case "language_ita":
					Tag = Enum.language_ita;
					break;
				case "language_tha":
					Tag = Enum.language_tha;
					break;
				case "language_fin":
					Tag = Enum.language_fin;
					break;
				case "language_hun":
					Tag = Enum.language_hun;
					break;
				case "language_ces":
					Tag = Enum.language_ces;
					break;
				case "language_tur":
					Tag = Enum.language_tur;
					break;
				case "language_ara":
					Tag = Enum.language_ara;
					break;
				case null:
					throw new ArgumentNullException(nameof(str));
				default:
					throw new ArgumentException("Invalid ApiUserTag");
			}
		}

		public override string ToString()
		{
			switch (Tag)
			{
				case Enum.admin_moderator:
					return "admin_moderator";
				case Enum.system_trust_legend:
					return "system_trust_legend";
				case Enum.system_trust_veteran:
					return "system_trust_veteran";
				case Enum.system_trust_intermediate:
					return "system_trust_intermediate";
				case Enum.system_trust_trusted:
					return "system_trust_trusted";
				case Enum.system_trust_known:
					return "system_trust_known";
				case Enum.system_trust_basic:
					return "system_trust_basic";
				case Enum.system_probable_troll:
					return "system_probable_troll";
				case Enum.system_troll:
					return "system_troll";
				case Enum.system_feedback_access:
					return "system_feedback_access";
				case Enum.system_scripting_access:
					return "system_scripting_access";
				case Enum.system_avatar_access:
					return "system_avatar_access";
				case Enum.system_world_access:
					return "system_world_access";
				case Enum.admin_scripting_access:
					return "admin_scripting_access";
				case Enum.admin_avatar_access:
					return "admin_avatar_access";
				case Enum.admin_world_access:
					return "admin_world_access";
				case Enum.show_social_rank:
					return "show_social_rank";
				case Enum.admin_avatar_restricted:
					return "admin_avatar_restricted";
				case Enum.admin_world_restricted:
					return "admin_world_restricted";
				case Enum.admin_lock_level:
					return "admin_lock_level";
				case Enum.admin_lock_tags:
					return "admin_lock_tags";
				case Enum.admin_official_thumbnail:
					return "admin_official_thumbnail";
				case Enum.language_eng:
					return "language_eng";
				case Enum.language_kor:
					return "language_kor";
				case Enum.language_rus:
					return "language_rus";
				case Enum.language_spa:
					return "language_spa";
				case Enum.language_por:
					return "language_por";
				case Enum.language_zho:
					return "language_zho";
				case Enum.language_deu:
					return "language_deu";
				case Enum.language_jpn:
					return "language_jpn";
				case Enum.language_fra:
					return "language_fra";
				case Enum.language_swe:
					return "language_swe";
				case Enum.language_nld:
					return "language_nld";
				case Enum.language_pol:
					return "language_pol";
				case Enum.language_dan:
					return "language_dan";
				case Enum.language_nor:
					return "language_nor";
				case Enum.language_ita:
					return "language_ita";
				case Enum.language_tha:
					return "language_tha";
				case Enum.language_fin:
					return "language_fin";
				case Enum.language_hun:
					return "language_hun";
				case Enum.language_ces:
					return "language_ces";
				case Enum.language_tur:
					return "language_tur";
				case Enum.language_ara:
					return "language_ara";
				default:
					return "invalid";
			}
		}

		public bool Equals(Enum other)
		{
			return Tag == other;
		}

		public bool Equals(ApiUserTag other)
		{
			return Tag == other.Tag;
		}

		public int CompareTo(Enum other)
		{
			return Tag - other;
		}

		public int CompareTo(ApiUserTag other)
		{
			return Tag - other.Tag;
		}

		public int CompareTo(object obj)
		{
			if (obj is Enum)
				return Tag - (Enum) obj;
			if (obj is ApiUserTag)
				return Tag - ((ApiUserTag) obj).Tag;

			throw new ArgumentException("Value is not a Guid or VrcId");
		}
	}
}