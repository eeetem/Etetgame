﻿using System;
using Riptide;

namespace DefconNull.Networking;
	public class LobbyData:IMessageSerializable
	{
		public string Name { get; set; } = "";

		protected bool Equals(LobbyData other)
		{
			return string.Equals(Name, other.Name, StringComparison.InvariantCulture) && string.Equals(MapName, other.MapName, StringComparison.InvariantCulture) && PlayerCount == other.PlayerCount && Spectators == other.Spectators && string.Equals(GameState, other.GameState, StringComparison.InvariantCulture) && Port == other.Port && HasPassword == other.HasPassword;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((LobbyData) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = StringComparer.InvariantCulture.GetHashCode(Name);
				hashCode = (hashCode * 397) ^ StringComparer.InvariantCulture.GetHashCode(MapName);
				hashCode = (hashCode * 397) ^ PlayerCount;
				hashCode = (hashCode * 397) ^ Spectators;
				hashCode = (hashCode * 397) ^ StringComparer.InvariantCulture.GetHashCode(GameState);
				hashCode = (hashCode * 397) ^ Port;
				hashCode = (hashCode * 397) ^ HasPassword.GetHashCode();
				return hashCode;
			}
		}

		public string MapName { get; set; } = "";
		public int PlayerCount { get; set; }
		public int Spectators { get; set; }
		public string GameState { get; set; } = "";
		public int Port{ get; set; }
		public bool HasPassword{ get; set; }
		//spectators
		public LobbyData(string name, int port)
		{
			Name = name;
			Port = port;
			HasPassword = false;
			MapName = "Unknown";
			PlayerCount = 0;
			Spectators = 0;
			GameState = "Starting...";
		}

		public LobbyData()
		{
			
		}

		public void Serialize(Message message)
		{
			message.Add(Name);
			message.Add(MapName);
			message.Add(PlayerCount);
			message.Add(Spectators);
			message.Add(GameState);
			message.Add(Port);
			message.Add(HasPassword);
		}

		public void Deserialize(Message message)
		{
			Name = message.GetString();
			MapName = message.GetString();
			PlayerCount = message.GetInt();
			Spectators = message.GetInt();
			GameState = message.GetString();
			Port = message.GetInt();
			HasPassword = message.GetBool();
		}
	}


