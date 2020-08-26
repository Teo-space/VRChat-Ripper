using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using VRC;
using VRC.API;

namespace Ripper
{
	public class OldVrcDb
	{
		private SQLiteConnection connection;

		private ConcurrentDictionary<VrcId, (long dbId, int version, bool isRemoved)> idCache = new ConcurrentDictionary<VrcId, (long, int, bool)>();

		private ConcurrentDictionary<string, long> developerEnumCache = new ConcurrentDictionary<string, long>();
		private ConcurrentDictionary<string, long> publicityEnumCache = new ConcurrentDictionary<string, long>();
		private ConcurrentDictionary<string, long> tagEnumCache = new ConcurrentDictionary<string, long>();

		private static string SqlEscapeString(string input)
		{
			if (input == null)
				return "NULL";

			return input.Replace("'", "''");
		}

		private static string MakeSqlSafeDateTime(DateTime? input)
		{
			return input == null ? "0" : $"{input?.ToBinary()}";
		}

		private DateTime? RevertSqlSafeDateTime(string input)
		{
			return input == null || input == "NULL" ? (DateTime?)null : DateTime.FromBinary(long.Parse(input));
		}

		public bool Open(string path)
		{
				if (InternalIsOpen())
					return false;

				connection = new SQLiteConnection(path);
				connection.Open();

				return true;
		}

		public void Close()
		{
				if (!InternalIsOpen())
				{
					connection.Close();
					connection.Dispose();
				}
				connection = null;
		}

		public ConnectionState State
		{
			get
			{
					return connection?.State ?? ConnectionState.Closed;
			}
		}

		private bool InternalIsOpen()
		{
			return connection != null && connection.State == ConnectionState.Open;
		}

		public bool Init()
		{
				if (!InternalIsOpen())
					return false;

				SQLiteCommand command;
				SQLiteDataReader reader;
				using (command = new SQLiteCommand(connection))
				{
					#region Create_Tables
					command.CommandText =
						"CREATE TABLE IF NOT EXISTS config(key TEXT NOT NULL UNIQUE, value TEXT NOT NULL);" +

						"CREATE TABLE IF NOT EXISTS enum_tag(id INTEGER PRIMARY KEY AUTOINCREMENT, value TEXT NOT NULL UNIQUE);" +
						"CREATE TABLE IF NOT EXISTS enum_developer(id INTEGER PRIMARY KEY AUTOINCREMENT, value TEXT NOT NULL UNIQUE);" +
						"CREATE TABLE IF NOT EXISTS enum_publicity(id INTEGER PRIMARY KEY AUTOINCREMENT, value TEXT NOT NULL UNIQUE);" +

						"CREATE TABLE IF NOT EXISTS assetbundles(id INTEGER PRIMARY KEY AUTOINCREMENT, vrc_id VARCHAR(22) NOT NULL UNIQUE, is_removed BOOLEAN NOT NULL);" +
						"CREATE TABLE IF NOT EXISTS directories(id INTEGER PRIMARY KEY AUTOINCREMENT, directory TEXT NOT NULL UNIQUE, weight INTEGER NOT NULL);" +

						// id				Internal database id
						// vrc_id			VRChat Id
						// is_legacy		Is old type of userid (10 char id)
						// is_removed		Has this been removed from vrchat servers?
						// name				Initial name
						// displayname		Current name
						// trust_level		Trust rank [ avoid | nuisance | visitor | new user | user | known | trusted | veteran | legend ]
						// developer_type	Type of developer [ none | trusted | internal | moderator ]
						"CREATE TABLE IF NOT EXISTS users(id INTEGER PRIMARY KEY AUTOINCREMENT, vrc_id VARCHAR(22) NOT NULL UNIQUE, is_legacy BOOLEAN NOT NULL, is_removed BOOLEAN NOT NULL, name TEXT, displayname TEXT, trust_level INTEGER, developer_type INTEGER, FOREIGN KEY(developer_type) REFERENCES enum_developer(id));" +
						"CREATE TABLE IF NOT EXISTS users_tags_map(id INTEGER NOT NULL, tag INTEGER NOT NULL, FOREIGN KEY(id) REFERENCES users(id), FOREIGN KEY(tag) REFERENCES enum_tag(id));" +

						// id				Internal database id
						// vrc_id			VRChat Id
						// is_legacy		Is old type of worldId (wld_)
						// is_removed		Has this been removed from vrchat servers?
						// name				Name
						// capacity			Amount of player-slots
						// version			Latest stored world revision
						// author			Internal database id of author
						// publicity		Release status [ public | private | hidden | all ]
						// created_at		Creation date
						// updated_at		Latest date updated
						"CREATE TABLE IF NOT EXISTS worlds(id INTEGER PRIMARY KEY AUTOINCREMENT, vrc_id VARCHAR(22) NOT NULL UNIQUE, is_legacy BOOLEAN NOT NULL, is_removed BOOLEAN NOT NULL, name TEXT, capacity INTEGER, version INTEGER, author INTEGER, publicity INTEGER, created_at INTEGER, updated_at INTEGER, FOREIGN KEY(author) REFERENCES users(id), FOREIGN KEY(publicity) REFERENCES enum_publicity(id));" +
						"CREATE TABLE IF NOT EXISTS worlds_assetbundle_map(world INTEGER NOT NULL, assetbundle INTEGER NOT NULL, FOREIGN KEY(world) REFERENCES worlds(id), FOREIGN KEY(assetbundle) REFERENCES assetbundles(id));" +
						"CREATE TABLE IF NOT EXISTS worlds_directory_map(world INTEGER NOT NULL, directory INTEGER NOT NULL, FOREIGN KEY(world) REFERENCES worlds(id), FOREIGN KEY(directory) REFERENCES directories(id));" +
						"CREATE TABLE IF NOT EXISTS worlds_tags_map(id INTEGER NOT NULL, tag INTEGER NOT NULL, FOREIGN KEY(id) REFERENCES worlds(id), FOREIGN KEY(tag) REFERENCES enum_tag(id));" +

						// id				Internal database id
						// vrc_id			VRChat Id
						// is_removed		Has this been removed from vrchat servers?
						// name				Latest Name
						// version			Latest stored avatar revision
						// author			Internal database id of author
						// publicity		Release status [ public | private | hidden | all ]
						// created_at		Creation date
						// updated_at		Latest date updated
						"CREATE TABLE IF NOT EXISTS avatars(id INTEGER PRIMARY KEY AUTOINCREMENT, vrc_id VARCHAR(22) NOT NULL UNIQUE, is_removed BOOLEAN NOT NULL, name TEXT, version INTEGER, author INTEGER, publicity INTEGER, created_at INTEGER, updated_at INTEGER, FOREIGN KEY(author) REFERENCES users(id), FOREIGN KEY(publicity) REFERENCES enum_publicity(id));" +
						"CREATE TABLE IF NOT EXISTS avatars_assetbundle_map(avatar INTEGER NOT NULL, assetbundle INTEGER NOT NULL, FOREIGN KEY(avatar) REFERENCES avatars(id), FOREIGN KEY(assetbundle) REFERENCES assetbundles(id));" +
						"CREATE TABLE IF NOT EXISTS avatars_directory_map(avatar INTEGER NOT NULL, directory INTEGER NOT NULL, FOREIGN KEY(avatar) REFERENCES avatars(id), FOREIGN KEY(directory) REFERENCES directories(id));" +
						"CREATE TABLE IF NOT EXISTS avatars_tags_map(id INTEGER NOT NULL, tag INTEGER NOT NULL, FOREIGN KEY(id) REFERENCES avatars(id), FOREIGN KEY(tag) REFERENCES enum_tag(id));";

					_ = command.ExecuteNonQuery();
					#endregion
					#region developer

					command.CommandText = "SELECT * FROM enum_developer";
					using (reader = command.ExecuteReader())
					{
						while (reader.HasRows && reader.Read())
						{
							_ = developerEnumCache.TryAdd(reader.GetString(1), reader.GetInt64(0));
						}
					}
					#endregion
					#region publicity
					command.CommandText = "SELECT * FROM enum_publicity";
					using (reader = command.ExecuteReader())
					{
						while (reader.HasRows && reader.Read())
						{
							_ = publicityEnumCache.TryAdd(reader.GetString(1), reader.GetInt64(0));
						}
					}
					#endregion
					#region tags
					command.CommandText = "SELECT * FROM enum_tag";
					using (reader = command.ExecuteReader())
					{
						while (reader.HasRows && reader.Read())
						{
							_ = tagEnumCache.TryAdd(reader.GetString(1), reader.GetInt64(0));
						}
					}
					#endregion
					#region assetbundles
					command.CommandText = "SELECT id,vrc_id,is_removed FROM assetbundles";
					using (reader = command.ExecuteReader())
					{
						while (reader.HasRows && reader.Read())
						{
							_ = idCache.TryAdd(new VrcId(VrcId.VrcIdType.Avatar, reader.GetString(0)), (reader.GetInt64(1), -1, reader.GetBoolean(2)));
						}
					}
					#endregion
					#region avatars
					command.CommandText = "SELECT vrc_id,id,IFNULL(version,-1),is_removed FROM avatars";
					using (reader = command.ExecuteReader())
					{
						while (reader.HasRows && reader.Read())
						{
							_ = idCache.TryAdd(new VrcId(VrcId.VrcIdType.Avatar, reader.GetString(0)), (reader.GetInt64(1), reader.GetInt32(2), reader.GetBoolean(3)));
						}
					}
					#endregion
					#region users
					command.CommandText = "SELECT is_legacy,vrc_id,id,is_removed FROM users";
					using (reader = command.ExecuteReader())
					{
						while (reader.HasRows && reader.Read())
						{
							_ = idCache.TryAdd(new VrcId(reader.GetBoolean(0) ? VrcId.VrcIdType.LegacyUser : VrcId.VrcIdType.User, reader.GetString(1)), (reader.GetInt64(2), -1, reader.GetBoolean(3)));
						}
					}
					#endregion
					#region worlds
					command.CommandText = "SELECT is_legacy,vrc_id,id,IFNULL(version,-1),is_removed FROM worlds";
					using (reader = command.ExecuteReader())
					{
						while (reader.HasRows && reader.Read())
						{
							_ = idCache.TryAdd(new VrcId(reader.GetBoolean(0) ? VrcId.VrcIdType.LegacyWorld : VrcId.VrcIdType.World, reader.GetString(1)), (reader.GetInt64(2), reader.GetInt32(3), reader.GetBoolean(4)));
						}
					}
					#endregion
				}

				return true;
		}

		#region ConfigCmds
		public Dictionary<string, string> ConfigGet()
		{
			var config = new Dictionary<string, string>();

			if (!InternalIsOpen())
				return config;

			SQLiteCommand command;
			SQLiteDataReader reader;
			using (command = new SQLiteCommand(connection))
			{
				command.CommandText = "SELECT key,value FROM config";

				using (reader = command.ExecuteReader())
				{
					while (reader.HasRows && reader.Read())
						config.Add(reader.GetString(0), reader.GetString(1));
				}
			}

			return config;
		}
		public void ConfigSet(Dictionary<string, string> config)
		{
			if (config == null || config.Count == 0)
				return;

			if (!InternalIsOpen())
				return;

			SQLiteCommand command;
			using (command = new SQLiteCommand(connection))
			{
				foreach (var entry in config)
				{
					if (entry.Key == null || entry.Value == null)
						continue;

					String key = SqlEscapeString(entry.Key);
					String value = SqlEscapeString(entry.Value);

					command.CommandText =
						$"INSERT OR IGNORE INTO config(key, value) VALUES ('{key}','{value}')" +
						$"ON CONFLICT(key) DO UPDATE SET value='{value}'";
					_ = command.ExecuteNonQuery();
				}
			}
		}
		public void ConfigEntryAddOrUpdate(String key, String value)
		{
			if (key == null || value == null)
				return;
			key = SqlEscapeString(key);
			value = SqlEscapeString(value);

			if (!InternalIsOpen())
				return;

			SQLiteCommand command;
			using (command = new SQLiteCommand(connection))
			{
				command.CommandText =
					$"INSERT OR IGNORE INTO config(key, value) VALUES ('{key}','{value}')" +
					$"ON CONFLICT(key) DO UPDATE SET value='{value}'";
				_ = command.ExecuteNonQuery();
			}
		}
		public void ConfigEntryRemove(String key)
		{
			if (key == null)
				return;
			key = SqlEscapeString(key);

			if (!InternalIsOpen())
				return;

			SQLiteCommand command;
			using (command = new SQLiteCommand(connection))
			{
				command.CommandText = $"DELETE FROM config WHERE key='{key}'";
				_ = command.ExecuteNonQuery();
			}
		}
		#endregion

		public VrcId[] GetAllIds()
		{
			return idCache.Keys.ToArray();
		}

		public VrcId[] GetAllIds(bool isRemoved)
		{
			return idCache.Where(p => p.Value.isRemoved == isRemoved).Select(p => p.Key).ToArray();
		}

		private long InsertOrUpdateId(SQLiteCommand command, VrcId id, bool isRemoved)
		{
			int version = -1;
			var compressedId = id.Guid.ToString("N");

			switch (id.IdType)
			{
				case VrcId.VrcIdType.Avatar:
					command.CommandText =
						$"INSERT INTO avatars(vrc_id, is_removed) VALUES('{compressedId}',{(isRemoved)}) ON CONFLICT(vrc_id) DO UPDATE SET is_removed={isRemoved};" +
						$"SELECT id, version FROM avatars WHERE vrc_id='{compressedId}';";
					break;
				case VrcId.VrcIdType.LegacyWorld:
				case VrcId.VrcIdType.World:
					command.CommandText =
						$"INSERT INTO worlds(vrc_id, is_legacy, is_removed) VALUES('{compressedId}',{(id.IdType == VrcId.VrcIdType.LegacyWorld)},{isRemoved}) ON CONFLICT(vrc_id) DO UPDATE SET is_removed={isRemoved};" +
						$"SELECT id, version FROM worlds WHERE vrc_id='{compressedId}';";
					break;
				case VrcId.VrcIdType.User:
				case VrcId.VrcIdType.LegacyUser:
					command.CommandText =
						$"INSERT INTO users(vrc_id, is_legacy, is_removed) VALUES('{compressedId}',{id.IdType == VrcId.VrcIdType.LegacyUser},{isRemoved}) ON CONFLICT(vrc_id) DO UPDATE SET is_removed={isRemoved};" +
						$"SELECT id FROM users WHERE vrc_id='{compressedId}';";
					break;
				case VrcId.VrcIdType.File:
					command.CommandText =
						$"INSERT INTO unitypackages(vrc_id, is_removed) VALUES('{compressedId}',{isRemoved}) ON CONFLICT(vrc_id) DO UPDATE SET is_removed={isRemoved};" +
						$"SELECT id FROM unitypackages WHERE vrc_id='{compressedId}';";
					break;
				default:
					return -1;
			}

			using (var reader = command.ExecuteReader())
			{
				if (reader.HasRows && reader.Read())
				{
					long dbId = reader.GetInt64(0);

					if (reader.FieldCount == 2 && !reader.IsDBNull(1))
						version = reader.GetInt32(1);

					_ = idCache.TryAdd(id, (dbId, version, isRemoved));
					return dbId;
				}
			}
			return -1;
		}
		private long EnsurePublicityStatus(SQLiteCommand command, String publicityStatus)
		{
			command.CommandText =
				$"INSERT OR IGNORE INTO enum_publicity(value) VALUES('{publicityStatus}');" +
				$"SELECT id FROM enum_publicity WHERE value='{publicityStatus}';";

			using (var reader = command.ExecuteReader())
			{
				if (reader.HasRows && reader.Read())
				{
					long dbId = reader.GetInt64(0);
					_ = publicityEnumCache.TryAdd(publicityStatus, dbId);
					return dbId;
				}
			}
			return -1;
		}
		private long EnsureDeveloperType(SQLiteCommand command, String developerType)
		{
			command.CommandText =
				$"INSERT OR IGNORE INTO enum_developer(value) VALUES('{developerType}');" +
				$"SELECT id FROM enum_developer WHERE value='{developerType}';";

			using (var reader = command.ExecuteReader())
			{
				if (reader.HasRows && reader.Read())
				{
					long dbId = reader.GetInt64(0);
					_ = developerEnumCache.TryAdd(developerType, dbId);
					return dbId;
				}
			}
			return -1;
		}
		private long EnsureTag(SQLiteCommand command, String tag)
		{
			command.CommandText =
				$"INSERT OR IGNORE INTO enum_tag(value) VALUES('{tag}');" +
				$"SELECT id FROM enum_tag WHERE value='{tag}';";

			using (var reader = command.ExecuteReader())
			{
				if (reader.HasRows && reader.Read())
				{
					long dbId = reader.GetInt64(0);
					_ = tagEnumCache.TryAdd(tag, dbId);
					return dbId;
				}
			}
			return -1;
		}

		public bool TryGetDbId(VrcId id, out long dbId)
		{
			dbId = -1;
			string tableName;
			(long dbId, int version, bool isRemoved) data;
			string compressedId = id.Guid.ToString("N");

			switch (id.IdType)
			{
				case VrcId.VrcIdType.Avatar:
					tableName = "avatars";
					if (idCache.TryGetValue(id, out data))
					{
						dbId = data.dbId;
						return true;
					}
					break;
				case VrcId.VrcIdType.World:
				case VrcId.VrcIdType.LegacyWorld:
					tableName = "worlds";
					if (idCache.TryGetValue(id, out data))
					{
						dbId = data.dbId;
						return true;
					}
					break;
				case VrcId.VrcIdType.User:
				case VrcId.VrcIdType.LegacyUser:
					tableName = "users";
					if (idCache.TryGetValue(id, out data))
					{
						dbId = data.dbId;
						return true;
					}
					break;
				case VrcId.VrcIdType.File:
					tableName = "assetbundles";
					if (idCache.TryGetValue(id, out data))
					{
						dbId = data.dbId;
						return true;
					}
					break;
				default:
					return false;
			}


				if (!InternalIsOpen())
					return false;

				SQLiteCommand command;
				SQLiteDataReader reader;
				using (command = new SQLiteCommand(connection))
				{
					// Ensure userId
					command.CommandText = $"SELECT id FROM {tableName} WHERE vrc_id='{compressedId}'";

					using (reader = command.ExecuteReader())
					{
						if (reader.HasRows && reader.Read())
						{
							dbId = reader.GetInt64(0);
							return true;
						}
					}
				}

			return false;
		}
		public bool AddUser(ApiUser user)
		{
			var name = SqlEscapeString(user.username);
			bool isLegacy = user.id.IdType == VrcId.VrcIdType.LegacyUser;
			var displayName = SqlEscapeString(user.displayName);
			String compressedId = user.id.Guid.ToString("N");

			if (!developerEnumCache.TryGetValue(user.developerType, out long dbDevTypeId))
				dbDevTypeId = -1;

			if (!InternalIsOpen())
				return false;

			SQLiteCommand command;
			using (command = new SQLiteCommand(connection))
			{
				// Ensure developerTypes
				if (dbDevTypeId == -1)
					dbDevTypeId = EnsureDeveloperType(command, user.developerType);

				var trustLevel = 2; // visitor
				foreach (var tag in user.tags)
				{
					switch (tag.Tag)
					{
						case ApiUserTag.Enum.admin_moderator:
							trustLevel = 11; // Moderator
							break;
						case ApiUserTag.Enum.admin_scripting_access:
						case ApiUserTag.Enum.system_scripting_access:
							trustLevel = 10; // Scripting access
							continue;
						case ApiUserTag.Enum.system_legend:
						case ApiUserTag.Enum.system_trust_legend:
							if (trustLevel > 1 && trustLevel < 9)
								trustLevel = 9; // Legend user
							continue;
						case ApiUserTag.Enum.system_trust_veteran:
							if (trustLevel > 1 && trustLevel < 8)
								trustLevel = 8; // veteran user
							continue;
						case ApiUserTag.Enum.system_trust_intermediate:
							if (trustLevel > 1 && trustLevel < 7)
								trustLevel = 7; // trusted user
							continue;
						case ApiUserTag.Enum.system_trust_trusted:
							if (trustLevel > 1 || trustLevel < 6)
								trustLevel = 6; // known user
							continue;
						case ApiUserTag.Enum.system_trust_known:
							if (trustLevel > 1 || trustLevel < 5)
								trustLevel = 5; // user
							continue;
						case ApiUserTag.Enum.system_trust_basic:
							if (trustLevel == 2)
								trustLevel = 3; // new user
							continue;
						case ApiUserTag.Enum.system_probable_troll:
							if (trustLevel > 1 && trustLevel != 9)
								trustLevel = 1; // nuisance
							continue;
						case ApiUserTag.Enum.system_troll:
							if (trustLevel != 9)
								trustLevel = 0; // avoid me
							continue;
						default:
							continue;
					}

					break;
				}

				// Ensure user
				command.CommandText =
					$"INSERT INTO users(vrc_id, is_legacy, is_removed, name, displayname, trust_level, developer_type)" +
					$"VALUES ('{compressedId}',{isLegacy},0,'{name}','{displayName}',{trustLevel},{dbDevTypeId})" +
					$"ON CONFLICT(vrc_id) DO UPDATE SET is_removed=0,displayname='{displayName}',trust_level={trustLevel},developer_type={dbDevTypeId};" +
					$"SELECT id FROM users WHERE vrc_id='{compressedId}';";

				long dbId;
				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows || !reader.Read())
						return false;

					dbId = reader.GetInt64(0);
					_ = idCache.TryAdd(user.id, (dbId, -1, false));
				}

				// Ensure tags
				if (user.tags != null)
				{
					foreach (var tag in user.tags)
					{
						String tagStr = tag.ToString();

						if (!tagEnumCache.TryGetValue(tagStr, out long dbTagId))
							dbTagId = EnsureTag(command, tagStr);

						command.CommandText =
							$"INSERT INTO users_tags_map(id, tag) SELECT {dbId},{dbTagId} WHERE NOT EXISTS(SELECT * FROM users_tags_map WHERE id={dbId} AND tag={dbTagId})";
						_ = command.ExecuteNonQuery();
					}
				}
			}

			return true;
		}
		public bool AddAvatar(ApiAvatar avatar, bool overwrite = false)
		{
			var name = SqlEscapeString(avatar.name);
			var createdAt = MakeSqlSafeDateTime(avatar.created_at);
			var updatedAt = MakeSqlSafeDateTime(avatar.updated_at);
			String compressedId = avatar.id.Guid.ToString("N");

			if (!idCache.TryGetValue(avatar.id, out var avatarEntry))
			{
				avatarEntry.dbId = -1;
			}
			else if (avatarEntry.version == avatar.version && !avatarEntry.isRemoved)
			{
				if (!overwrite)
					return false;
			}

			avatarEntry.isRemoved = false;
			avatarEntry.version = avatar.version;

			if (!idCache.TryGetValue(avatar.authorId, out var authorEntry))
			{
				authorEntry.dbId = -1;
				authorEntry.version = -1;
				authorEntry.isRemoved = true;
			}

			if (!publicityEnumCache.TryGetValue(avatar.releaseStatus, out long dbPublicityId))
				dbPublicityId = -1;

				if (!InternalIsOpen())
					return false;

				SQLiteCommand command;
				using (command = new SQLiteCommand(connection))
				{
					// Ensure authorId
					if (authorEntry.dbId == -1)
						authorEntry.dbId = InsertOrUpdateId(command, avatar.authorId, false);

					// Ensure releaseStatus
					if (dbPublicityId == -1)
						EnsurePublicityStatus(command, avatar.releaseStatus);

					if (avatarEntry.dbId == -1)
					{
						// Ensure avatar
						command.CommandText =
							"INSERT INTO avatars(vrc_id, is_removed, name, version, author, publicity, created_at, updated_at) VALUES" +
							$"('{compressedId}',0,'{name}',{avatar.version},{authorEntry.dbId},{dbPublicityId},{createdAt},{updatedAt})" +
							$"ON CONFLICT(vrc_id) DO UPDATE SET is_removed=0,name='{name}',version={avatar.version},publicity={dbPublicityId},updated_at={updatedAt};" +
							$"SELECT id FROM avatars WHERE vrc_id='{compressedId}';";

						using (var reader = command.ExecuteReader())
						{
							if (!reader.HasRows || !reader.Read())
								return false;

							avatarEntry.dbId = reader.GetInt64(0);
							_ = idCache.AddOrUpdate(avatar.id, avatarEntry,
								(id, entry) =>
								{
									if (id == avatar.id && entry.dbId == avatarEntry.dbId)
										entry = avatarEntry;

									return entry;
								});
						}
					}
					else
					{
						// Update avatar
						command.CommandText = 
							$"UPDATE avatars SET name='{name}',version={avatar.version},publicity={dbPublicityId},updated_at={updatedAt} " +
							$"WHERE id={avatarEntry.dbId}";
						_ = command.ExecuteNonQuery();

						_ = idCache.AddOrUpdate(avatar.id, avatarEntry,
							(id, entry) =>
							{
								if (id == avatar.id && entry.dbId == avatarEntry.dbId)
									entry = avatarEntry;

								return entry;
							});
					}

					// Ensure tags
					if (avatar.tags != null)
					{
						foreach (var tag in avatar.tags)
						{
							if (!tagEnumCache.TryGetValue(tag.ToString(), out long dbTagId))
								dbTagId = EnsureTag(command, tag.ToString());

							command.CommandText = $"INSERT INTO avatars_tags_map(id, tag) SELECT {avatarEntry.dbId},{dbTagId} WHERE NOT EXISTS(SELECT * FROM avatars_tags_map WHERE id={avatarEntry.dbId} AND tag={dbTagId})";
							_ = command.ExecuteNonQuery();
						}
					}
				}

			return true;
		}
		public bool AddWorld(ApiWorld world, bool overwrite = false)
		{
			var name = SqlEscapeString(world.name);
			bool isLegacy = world.id.IdType == VrcId.VrcIdType.LegacyWorld;
			var createdAt = MakeSqlSafeDateTime(world.created_at);
			var updatedAt = MakeSqlSafeDateTime(world.updated_at);
			String compressedId = world.id.Guid.ToString("N");

			if (!idCache.TryGetValue(world.id, out var worldEntry))
			{
				worldEntry.dbId = -1;
			}
			else if (worldEntry.version == world.version && !worldEntry.isRemoved)
			{
				if (!overwrite)
					return false;
			}

			worldEntry.isRemoved = false;
			worldEntry.version = world.version;

			if (!idCache.TryGetValue(world.authorId, out var authorEntry))
			{
				authorEntry.dbId = -1;
				authorEntry.version = -1;
				authorEntry.isRemoved = true;
			}

			if (!publicityEnumCache.TryGetValue(world.releaseStatus, out long dbPublicityId))
				dbPublicityId = -1;

				if (!InternalIsOpen())
					return false;

				SQLiteCommand command;
				using (command = new SQLiteCommand(connection))
				{
					// Ensure authorId
					if (authorEntry.dbId == -1)
						authorEntry.dbId = InsertOrUpdateId(command, world.authorId, false);

					// Ensure releaseStatus
					if (dbPublicityId == -1)
						EnsurePublicityStatus(command, world.releaseStatus);

					if (worldEntry.dbId == -1)
					{
						// Ensure world
						command.CommandText =
							"INSERT INTO worlds(vrc_id, is_legacy, is_removed, name, capacity, version, author, publicity, created_at, updated_at) VALUES" +
							$"('{compressedId}',{isLegacy},0,'{name}',{world.capacity},{world.version},{authorEntry.dbId},{dbPublicityId},{createdAt},{updatedAt})" +
							$"ON CONFLICT(vrc_id) DO UPDATE SET is_removed=0,name='{name}',capacity={world.capacity},version={world.version},publicity={dbPublicityId},updated_at={updatedAt};" +
							$"SELECT id FROM worlds WHERE vrc_id='{compressedId}';";

						using (var reader = command.ExecuteReader())
						{
							if (!reader.HasRows || !reader.Read())
								return false;

							worldEntry.dbId = reader.GetInt64(0);
							_ = idCache.AddOrUpdate(world.id, worldEntry,
								(id, entry) =>
								{
									if (id == world.id && entry.dbId == worldEntry.dbId)
										entry = worldEntry;

									return entry;
								});
						}
					}
					else
					{
						// Update world
						command.CommandText = 
							$"UPDATE worlds SET is_removed=0,name='{name}',capacity={world.capacity},version={world.version},publicity={dbPublicityId},updated_at={updatedAt}" +
							$"WHERE id={worldEntry.dbId}";
						_ = command.ExecuteNonQuery();

						_ = idCache.AddOrUpdate(world.id, worldEntry,
							(id, entry) =>
							{
								if (id == world.id && entry.dbId == worldEntry.dbId)
									entry = worldEntry;

								return entry;
							});
					}

					// Ensure tags
					if (world.tags != null)
					{
						foreach (var tag in world.tags)
						{
							if (!tagEnumCache.TryGetValue(tag.ToString(), out long dbTagId))
								dbTagId = EnsureTag(command, tag.ToString());

							command.CommandText = $"INSERT INTO worlds_tags_map(id, tag) SELECT {worldEntry.dbId},{dbTagId} WHERE NOT EXISTS(SELECT * FROM worlds_tags_map WHERE id={worldEntry.dbId} AND tag={dbTagId})";
							_ = command.ExecuteNonQuery();
						}
					}
				}

			return true;
		}
		// TODO: FIXME
		/*
		public ApiUser GetUser(VrcId id)
		{
			String compressedId = id.Guid.ToString("N");
			bool isLegacy = id.IdType == VrcId.VrcIdType.LegacyUser;

			if (!InternalIsOpen())
				return null;

			SQLiteCommand command;
			using (command = new SQLiteCommand(connection))
			{
				command.CommandText = $"SELECT id,is_removed,name,displayname,trust_level,developer_type FROM users WHERE vrc_id='{compressedId}' AND is_legacy={isLegacy};";

				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows || !reader.Read())
						return false;

					dbId = reader.GetInt64(0);
					_ = idCache.TryAdd(user.id, (dbId, -1, false));
				}
			}
		}
		public ApiAvatar GetAvatar(VrcId id)
		{
			String compressedId = id.Guid.ToString("N");
			bool isLegacy = id.IdType == VrcId.VrcIdType.LegacyUser;

			if (!InternalIsOpen())
				return null;

			SQLiteCommand command;
			using (command = new SQLiteCommand(connection))
			{
				command.CommandText = $"SELECT id,is_removed,name,displayname,trust_level,developer_type FROM users WHERE vrc_id='{compressedId}' AND is_legacy={isLegacy};";

				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows || !reader.Read())
						return false;

					dbId = reader.GetInt64(0);
					_ = idCache.TryAdd(user.id, (dbId, -1, false));
				}
			}
		}
		public ApiWorld GetWorld(VrcId id)
		{
			String compressedId = id.Guid.ToString("N");
			bool isLegacy = id.IdType == VrcId.VrcIdType.LegacyUser;

			if (!InternalIsOpen())
				return null;

			SQLiteCommand command;
			using (command = new SQLiteCommand(connection))
			{
				command.CommandText = $"SELECT id,is_removed,name,displayname,trust_level,developer_type FROM users WHERE vrc_id='{compressedId}' AND is_legacy={isLegacy};";

				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows || !reader.Read())
						return false;

					dbId = reader.GetInt64(0);
					_ = idCache.TryAdd(user.id, (dbId, -1, false));
				}
			}
		}
		*/
		public void MarkAsDeleted(VrcId id)
		{
				if (!InternalIsOpen())
					return;

				using (var command = new SQLiteCommand(connection))
				{
					_ = InsertOrUpdateId(command, id, true);
				}
		}
	}
}
