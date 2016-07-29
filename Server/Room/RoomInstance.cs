using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using Server.Host;
using NetworkingMessages;
using NetworkingMessages.Messages;
using NetworkingMessages.Messages.Lobby;
using NetworkingMessages.Messages.ClientState;
using NetworkingMessages.Messages.Gameplay;
using NetworkingMessages.Messages.Gameplay.Players;

namespace Server.Room
{
    public class RoomInstance : PeerHandler
    {
        public long ID = long.MinValue;
        public string Name = string.Empty;

		public readonly int ControlChannel = 1;
		public readonly int PlayerInfoChannel = 2;
		public readonly int ChatChannel = 3;
		public readonly int PositionalUpdateChannel = 4;
		public readonly int ImportantPlayerUpdateChannel = 5;

        public class Player
        {
            public Peer NetPeer = null;

			public int PlayerID = int.MinValue;

            public class InboundMesage
            {
                public long TimeStamp = 0;
                public NetworkMessage Message = null;
            }

            public List<InboundMesage> PendingInboundMessages = new List<InboundMesage>();

            public void PushMessage(NetworkMessage msg)
            {
                if (msg == null || msg == NetworkMessage.Empty)
                    return;

                InboundMesage im = new InboundMesage();
                im.TimeStamp = System.DateTime.UtcNow.Ticks;
                im.Message = msg;

                lock (PendingInboundMessages)
                    PendingInboundMessages.Add(im);
            }

            public InboundMesage PopMessage()
            {
                lock (PendingInboundMessages)
                {
                    if (PendingInboundMessages.Count == 0)
                        return null;
                    InboundMesage im = PendingInboundMessages[0];
                    PendingInboundMessages.RemoveAt(0);
                    return im;
                }
            }
        }

        public Dictionary<long, Player> Players = new Dictionary<long, Player>();

		private int LastPlayerID = 0;

        public RoomInstance() { }
        public RoomInstance(string name) { Name = name; }

        public virtual void Startup()
        {

        }

		protected void SendToAll(NetworkingMessages.Messages.NetworkMessage message)
		{
			SendToAll(message, NetDeliveryMethod.ReliableOrdered, ControlChannel);
		}

		protected virtual void SendToAll(NetworkingMessages.Messages.NetworkMessage message, NetDeliveryMethod method, int channel)
		{
			foreach(var p in Players.Values)
				p.NetPeer.SendMessage(message, method, channel);
		}

		public virtual void AddPlayer(Peer peer)
        {
            Player p = new Player();
            p.NetPeer = peer;
            peer.Handler = this;

            lock(Players)
			{
				LastPlayerID++;
				p.PlayerID = LastPlayerID;
				Players.Add(peer.ID, p);
			}

            peer.SendMessage(SetClientState.PlayingState);

			// tell them WHAT they are playing
			var joinMsg = new GameRoomJoinedMessage();
			joinMsg.GameID = ID;
			joinMsg.GameName = Name;
			joinMsg.PlayerID = p.PlayerID;

			// these need to go in the same channel as the state change so they get in before normal updates that don't matter
			peer.SendMessage(joinMsg);

			UpdatePlayerInfoMessage msg = new UpdatePlayerInfoMessage();
			msg.PlayerID = p.PlayerID;
			msg.DisplayName = p.NetPeer.DisplayName;
			SendToAll(msg);
		}

		protected void SendPlayerUpdate(Player player)
		{
			UpdatePlayerInfoMessage msg = new UpdatePlayerInfoMessage();
			msg.PlayerID = player.PlayerID;
			msg.DisplayName = player.NetPeer.DisplayName;
			SendToAll(msg, NetDeliveryMethod.ReliableUnordered, PlayerInfoChannel);
		}

        protected Player GetPlayerFromPeer(Peer peer)
        {
            lock(Players)
            {
                if (Players.ContainsKey(peer.ID))
                    return Players[peer.ID];

                return null;
            }
        }

        public virtual void PeerReceiveData(NetIncomingMessage msg, Peer peer)
        {
            Player player = GetPlayerFromPeer(peer);
            if (player != null)
                player.PushMessage(MessageFactory.ParseMessage(msg));
        }

        public virtual void PeerDisconnected(string reason, Peer peer)
        {
            var p = GetPlayerFromPeer(peer);
            if (p == null)
                return;

            PushLeaveMessage(p);
			RemovePlayer(peer.ID);
		}

		protected void RemovePlayer(long id)
		{
			lock(Players)
			{
				if(Players.ContainsKey(id))
					Players.Remove(id);
			}
		}

        protected List<Player> DisconenctingPlayers = new List<Player>();

        protected void PushLeaveMessage(Player p)
        {
            lock (DisconenctingPlayers)
                DisconenctingPlayers.Add(p);
        }

        public virtual void DisconnectPeer(string reason, Peer peer)
        {
            peer.SocketConnection.Disconnect(reason);
        }

        public virtual void Update()
        {
            lock (DisconenctingPlayers)
            {
				foreach(var p in DisconenctingPlayers)
					SendPlayerPartMessage(p);

				DisconenctingPlayers.Clear();
            }

            // iterate and pop the messages
            foreach (var p in Players.Values)
                ProcessPendingPlayerMessages(p);

            // process the world state

        }

		protected virtual void SendPlayerPartMessage(Player p)
		{
			RemovePlayerMessage msg = new RemovePlayerMessage();
			msg.PlayerID = p.PlayerID;
			SendToAll(msg, NetDeliveryMethod.ReliableUnordered, PlayerInfoChannel);
		}

        public virtual void Shutdown()
        {

        }

        public static int MaxMessagsToProcess = 10;

        protected virtual void ProcessPendingPlayerMessages(Player player)
        {
            int msgCount = 0;

            Player.InboundMesage msg = player.PopMessage();
            while (msg != null && msgCount < MaxMessagsToProcess)
            {
                msgCount++;

                NetworkingMessages.Messages.NetworkMessage message = msg.Message;

				if (message as JoinLobbyMessage != null)
				{
					RemovePlayer(player.NetPeer.ID);
					SendPlayerPartMessage(player);
					Lobby.Manager.TransferPeerToLobby(player.NetPeer, (message as JoinLobbyMessage).LobbyName);
					return; // we are done with them, this should be the last message anyone cares about before the state change, so just ditch em.
				}
				

                msg = player.PopMessage();
            }
        }
    }
}
