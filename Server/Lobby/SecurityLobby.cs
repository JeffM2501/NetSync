using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

using Lidgren.Network;
using NetworkingMessages.Messages.Connection;
using NetworkingMessages.Messages.Authentication;
using NetworkingMessages.Messages.ClientState;

using Server.Host;
using Server.Security;

namespace Server.Lobby
{
    public class SecurityLobby : LobbyInstance
    {
        // attribute strings
        public static string IdentificationAttribute = "SecurityIdentStatus";
        public static string IdentificationTokenAttribute = "SecurityIdentToken";
        public static string AuthorizationAttribute = "SecurityAuthStatus";
        public static string BannedAttribute = "SecuityBanned";

        public static string AuthorizationCompleteAttribute = "SecuityCheckAuthorized";

        public BanList Bans = new BanList();

        protected AuthenticationProcessor AuthProcessor = new AuthenticationProcessor();
        protected DNSLookupProcessor DNSProcessor = new DNSLookupProcessor();

        public SecurityLobby()
        {
            Name = "Security";
        }

        public override void Shutdown()
        {
            DNSProcessor.Shutdown();
            AuthProcessor.Shutdown();
        }

        public override void AddPeer(Peer peer)
        {
            base.AddPeer(peer);

            peer.SendMessage(SetClientState.AuthState);

            peer.SetAttribute(AuthorizationAttribute, 0);
            peer.SetAttribute(AuthorizationCompleteAttribute, false);

            peer.UserID = string.Empty;
            peer.DisplayName = string.Empty;

            if (peer.GetAttributeB(BannedAttribute))
				DisconnectPeer("Previously Banned", peer);
			else
            {
                var ban = Bans.GetIPBan(peer.SocketConnection.RemoteEndPoint.Address.ToString());
                if (ban == null)
                    peer.SetAttribute(BannedAttribute, false);
                else
                {
                    peer.SetAttribute(BannedAttribute, true);
					DisconnectPeer(ban.Reason, peer);
					return;
                }

				DNSProcessor.PushDNSLookup(peer);

				peer.SetAttribute(IdentificationAttribute, 0);
				peer.SendMessage(RequestClientVersionMessage.Request, NetDeliveryMethod.ReliableOrdered,1);
			}
        }

        public override void PeerDisconnected(string reason, Peer peer)
        {
            // kill any lookup jobs we have for that peer

            DNSProcessor.RemoveAllJobsForPeer(peer);
            AuthProcessor.RemoveAllJobsForPeer(peer);
        }

		protected ClientVersionMessage ServerVersionMessage = new ClientVersionMessage();

		public override void PeerReceiveMessage(NetworkingMessages.Messages.NetworkMessage msg, Peer peer)
        {
			ClientVersionMessage versMsg = msg as ClientVersionMessage;
			if (versMsg != null)
			{
				if(ServerVersionMessage.ClientProduct != versMsg.ClientProduct || ServerVersionMessage.ClientProtocol != versMsg.ClientProtocol )
					DisconnectPeer("Incompatible Version", peer);
				else
				{
					if (peer.GetAttributeD(IdentificationAttribute) == 0)
						peer.SetAttribute(IdentificationAttribute, 1);
					peer.SendMessage(RequestAuthentication.Request, NetDeliveryMethod.ReliableOrdered, 1);
				}

				return;
			}

			AuthenticationMessage authMessage = msg as AuthenticationMessage;
			if(authMessage != null)
			{
				if(authMessage.AuthenticationToken == string.Empty || authMessage.UserID == string.Empty)
					DisconnectPeer("Invalid Authentication", peer);
				else
				{
					if(peer.GetAttributeD(IdentificationAttribute) == 1)
						peer.SetAttribute(IdentificationAttribute, 2);

                    AuthProcessor.PushAuthLookup(peer, string.Empty, authMessage.UserID, authMessage.AuthenticationToken);
				}

				return;
			}
		}

        public static int MaxJobsToFinalize = 10;

        public override void Update()
        {
            base.Update();
            DNSProcessor.CheckForDeadJobs();
            AuthProcessor.CheckForDeadJobs();

            DNSLookupProcessor.DNSLookupJob job = DNSProcessor.PopCompletedJob();

            int i = 0;
            while (job != null && i > MaxJobsToFinalize)
            {
                i++;

                Peer p = FindPeer(job.PeerID);
                if (p != null)
                {
                    var b = Bans.GetHostBan(job.HostInfo.HostName);
                    if (b == null)
                        p.SetAttribute(AuthorizationAttribute, 1);
                    else
                    {
                        p.SetAttribute(BannedAttribute, true);
                        p.SocketConnection.Disconnect(b.Reason);
                    }
                }

                job = DNSProcessor.PopCompletedJob();
            }

            AuthenticationProcessor.AuthenticationJob autJob = AuthProcessor.PopCompletedJob();

            i = 0;
            while (autJob != null && i > MaxJobsToFinalize)
            {
                i++;

                Peer p = FindPeer(autJob.PeerID);
                if (p != null)
                {
                    // for now just accept it; TODO, check the results
                    p.UserID = autJob.UserID;
                    p.DisplayName = "Player_" + new Random().Next().ToString();
                    p.SetAttribute(IdentificationAttribute, 3);
                }

                autJob = AuthProcessor.PopCompletedJob();
            }

            // check to see if anyone needs a transfer

            foreach(var p in LobbyPlayers.Values.ToArray())
            {
                // if they have passed all the checks
                if (p.GetAttributeD(AuthorizationAttribute) == 1 && p.GetAttributeD(IdentificationAttribute) == 3 && !p.GetAttributeB(BannedAttribute))
                {
                    // send them on there way
                    p.SetAttribute(AuthorizationCompleteAttribute, true);
                    LobbyPlayers.Remove(p.ID);

                    Manager.TransferPeerToLobby(p, string.Empty);   // let the manager put them in a default lobby
                }
            }
        }
    }
}
