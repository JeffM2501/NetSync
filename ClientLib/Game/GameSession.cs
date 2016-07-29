using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;
using NetworkingMessages.Messages.Gameplay;
using NetworkingMessages.Messages.Gameplay.Players;

namespace ClientLib.Game
{
	public class GameSession
	{
		public long GameID = long.MinValue;
		public string GameName = string.Empty;

		public class Player
		{
			public int PlayerID = int.MinValue;
			public string DisplayName = string.Empty;

			public Player(int id) { PlayerID = id; }
		}

		public class LocalPlayer : Player
		{
			public LocalPlayer(int id) : base(id) { }
		}

		public Dictionary<int, Player> PlayerList = new Dictionary<int, Player>();
		public LocalPlayer MyPlayer = null;

		protected Player GetPlayer(int id)
		{
			lock(PlayerList)
			{
				if(!PlayerList.ContainsKey(id))
					PlayerList.Add(id,new Player(id));
				return PlayerList[id];
			}
		}

		public void ProcessMessage(NetworkMessage msg)
		{
			if (GameID == long.MinValue)	// can't do anything until we get our join message
			{
				GameRoomJoinedMessage joinMessage = msg as GameRoomJoinedMessage;
				if (joinMessage != null)
				{
					GameID = joinMessage.GameID;
					GameName = joinMessage.GameName;

					MyPlayer = new LocalPlayer(joinMessage.PlayerID);
					MyPlayer.DisplayName = "LocalPlayer"; // will be set by global update

					lock(PlayerList)
						PlayerList.Add(MyPlayer.PlayerID, MyPlayer);
				}
				return;
			}

			if(ProcessPlayerListMessages(msg))
				return;	
		}

		public class PlayerEventArgs : EventArgs
		{
			public Player AffectedPlayer = null;

			public PlayerEventArgs(Player p) { AffectedPlayer = p; }
		}

		public event EventHandler<PlayerEventArgs> PlayerRemoved = null;
		public event EventHandler<PlayerEventArgs> PlayerUpdated = null;

		protected bool ProcessPlayerListMessages(NetworkMessage msg)
		{
			UpdatePlayerInfoMessage updateMsg = msg as UpdatePlayerInfoMessage;
			if(updateMsg != null)
			{
				var p = GetPlayer(updateMsg.PlayerID);
				p.DisplayName = updateMsg.DisplayName;

				PlayerUpdated?.Invoke(this, new PlayerEventArgs(p));
			}

			RemovePlayerMessage removeMsg = msg as RemovePlayerMessage;
			if(removeMsg != null)
			{
				Player p = null;
				lock(PlayerList)
				{
					if (PlayerList.ContainsKey(removeMsg.PlayerID))
					{
						p = PlayerList[removeMsg.PlayerID];
						PlayerList.Remove(removeMsg.PlayerID);
					}
				}
				if (p != null)
					PlayerRemoved?.Invoke(this, new PlayerEventArgs(p));

				if(p == MyPlayer)
				{
					// WTF? we got removed? tell someone?

					MyPlayer = null;
				}
			}
			return false;
		}
	}
}
