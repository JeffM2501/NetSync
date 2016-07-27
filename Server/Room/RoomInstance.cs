using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using Server.Host;
using NetworkingMessages;
using NetworkingMessages.Messages;
using NetworkingMessages.Messages.ClientState;

namespace Server.Room
{
    public class RoomInstance : PeerHandler
    {
        public long ID = long.MinValue;
        public string Name = string.Empty;

        public class Player
        {
            public Peer NetPeer = null;

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

        public RoomInstance() { }
        public RoomInstance(string name) { Name = name; }

        public virtual void Startup()
        {

        }

        public virtual void AddPlayer(Peer peer)
        {
            Player p = new Player();
            p.NetPeer = peer;
            peer.Handler = this;

            lock(Players)
                Players.Add(peer.ID, p);

            peer.SendMessage(SetClientState.PlayingState);
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
            lock (Players)
            {
                if (Players.ContainsKey(peer.ID))
                    Players.Remove(peer.ID);
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
                foreach (var p in DisconenctingPlayers)
                {
                    // send a disco message
                }
                DisconenctingPlayers.Clear();
            }

            // iterate and pop the messages
            foreach (var p in Players.Values)
                ProcessPendingPlayerMessages(p);

            // process the world state

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

                msg = player.PopMessage();
            }
        }
    }
}
