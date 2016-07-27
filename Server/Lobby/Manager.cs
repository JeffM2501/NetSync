using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using Server.Host;

namespace Server.Lobby
{
    public static class Manager
    {
        private static SecurityLobby SecurityAlcove = new SecurityLobby();

        private static LobbyInstance CurrentLobby = new LobbyInstance();
        private static List<LobbyInstance> ActiveLobbies = new List<LobbyInstance>();

        static Manager()
        {
            ActiveLobbies.Add(CurrentLobby);
        }

        public static Peer AcceptPeer(NetConnection connection)
        {
            Peer p = new Peer();
            p.Handler = SecurityAlcove;
            p.SocketConnection = connection;

            // assign them to the security check lobby, they will be transfered when validated
            SecurityAlcove.AddPeer(p);

            return p;
        }

        /// <summary>
        /// Transfers a peer to a new lobby. Assumes the peer has already been removed from the old lobby (but the handler is still active)
        /// </summary>
        /// <param name="peer"> who to move</param>
        /// <param name="lobbyName">the name of where to move them</param>
        public static void TransferPeerToLobby(Peer peer, string lobbyName)
        {
            LobbyInstance targetLobby = null;

            if (lobbyName == SecurityAlcove.Name)
                targetLobby = SecurityAlcove;

            if (lobbyName == string.Empty)
                targetLobby = CurrentLobby;
            else
                targetLobby = ActiveLobbies.Find(x => x.Name == lobbyName);

            if (targetLobby == null)
            {
                if (peer.Handler as LobbyInstance != null)          // we can't go where they wanted, send them back to where they came from
                    targetLobby = peer.Handler as LobbyInstance;
                else
                {
                    peer.SocketConnection.Disconnect("Invalid Lobby Transfer");
                    return;
                }
            }

            targetLobby.AddPeer(peer);
        }

        public static void UpdateLobbies()
        {
            SecurityAlcove.Update();
            foreach (var l in ActiveLobbies)
                l.Update();
        }
    }
}
