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

        public virtual void PeerReceiveData(NetIncomingMessage msg, Peer peer)
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

        public virtual void Update()
        {

        }
    }
}
