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

        void PeerHandler.Disconnected(string reason, Peer peer)
        {
            PeerDisconnected(reason, peer);
        }

        void PeerHandler.ReceiveData(NetIncomingMessage msg, Peer peer)
        {
            PeerReceiveData(msg, peer);
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


        protected virtual void PeerDisconnected(string reason, Peer peer)
        {
            throw new NotImplementedException();
        }

        protected virtual void PeerReceiveData(NetIncomingMessage msg, Peer peer)
        {
            throw new NotImplementedException();
        }

        public virtual void Update()
        {

        }
    }
}
