using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;
using Server.Host;

namespace Server.Lobby
{
    public class LobbyInstance : PeerHandler
    {
        public string Name = string.Empty;

        public Dictionary<long, Peer> LobbyPlayers = new Dictionary<long, Peer>();

		public virtual void PeerDisconnected(string reason, Peer peer)
        {
			LobbyPlayers.Remove(peer.SocketConnection.RemoteUniqueIdentifier);
        }

        public void PeerReceiveData(NetIncomingMessage msg, Peer peer)
        {
            NetworkingMessages.Messages.NetworkMessage inMsg = NetworkingMessages.MessageFactory.ParseMessage(msg);
            if (inMsg == null)
                return;
            PeerReceiveMessage(inMsg, peer);
        }

        public virtual void PeerReceiveMessage(NetworkingMessages.Messages.NetworkMessage msg, Peer peer)
        {

        }

        public virtual void DisconnectPeer(string reason, Peer peer)
		{
			peer.SocketConnection.Disconnect(reason);
		}

		public virtual void AddPeer(Peer peer)
        {
            lock (LobbyPlayers)
                LobbyPlayers.Add(peer.SocketConnection.RemoteUniqueIdentifier,peer);
        }

        public virtual Peer FindPeer(long id)
        {
            lock (LobbyPlayers)
            {
                if (LobbyPlayers.ContainsKey(id))
                    return LobbyPlayers[id];
                return null;
            }
        }

        public virtual void RemovePeer(Peer peer)
        {
            lock (LobbyPlayers)
            {
                if (LobbyPlayers.ContainsKey(peer.ID))
                    LobbyPlayers.Remove(peer.ID);

                peer.Handler = null;
            }
        }

        public virtual void Update()
        {

        }

        public virtual void Shutdown()
        {

        }

        protected void SendToAll(NetworkingMessages.Messages.NetworkMessage message)
        {
            SendToAll(message, NetDeliveryMethod.ReliableOrdered, 1);
        }

        protected virtual void SendToAll(NetworkingMessages.Messages.NetworkMessage message, NetDeliveryMethod method, int channel)
        {
            foreach (var p in LobbyPlayers.Values)
                p.SendMessage(message, method, channel);
        }
    }
}
