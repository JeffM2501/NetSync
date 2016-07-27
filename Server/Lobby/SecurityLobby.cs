﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

using Lidgren.Network;
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

        public BanList Bans = new BanList();

        public class DNSLookupJob
        {
            public long PeerID = long.MaxValue;
            public IPAddress Address = null;

            public IPHostEntry HostInfo = null;
        }

        private List<DNSLookupJob> PendingDNSLookups = new List<DNSLookupJob>();
        private List<DNSLookupJob> CompletedDNSLookups = new List<DNSLookupJob>();

        public static int MaxDNSThreds = 2;

        private List<Thread> DNSProcessThreads = new List<Thread>();

        public SecurityLobby()
        {
            Name = "Security";
        }

        public void Shutdown()
        {
            lock(DNSProcessThreads)
            {
                foreach (var t in DNSProcessThreads)
                    t.Abort();

                DNSProcessThreads.Clear();
            }
        }

        public override void AddPeer(Peer peer)
        {
            base.AddPeer(peer);

            peer.SetAttribute(AuthorizationAttribute, 0);

            if (peer.GetAttributeB(BannedAttribute))
                peer.SocketConnection.Disconnect("Banned");
            else
            {
                var ban = Bans.GetIPBan(peer.SocketConnection.RemoteEndPoint.Address.ToString());
                if (ban == null)
                    peer.SetAttribute(BannedAttribute, false);
                else
                {
                    peer.SetAttribute(BannedAttribute, true);
                    peer.SocketConnection.Disconnect(ban.Reason);
                }

                // start a job to validate the host mask and authentication status

            }
        }

        protected override void PeerDisconnected(string reason, Peer peer)
        {
            // kill any lookup jobs we have
        }

        protected override void PeerReceiveData(NetIncomingMessage msg, Peer peer)
        {
            // only check for messages we know unauthorized supplicant can send
        }

        public override void Update()
        {
            base.Update();
            CheckForDeadJobs();

            int dnsProcessCount = 0;
            DNSLookupJob job = PopCompletedDNSJob();

            while (job != null && dnsProcessCount > MaxDNSThreds)
            {
                dnsProcessCount++;

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

                job = PopCompletedDNSJob();
            }

            // check to see if anyone needs a transfer
        }

        protected DNSLookupJob PopCompletedDNSJob()
        {
            DNSLookupJob job = null;
            lock (CompletedDNSLookups)
            {
                if (CompletedDNSLookups.Count == 0)
                    return null;

                job = CompletedDNSLookups[0];
                CompletedDNSLookups.RemoveAt(0);
            }

            return job;
        }

        protected void CheckForDeadJobs()
        {
            bool haveThreads = false;
            lock (DNSProcessThreads)
                haveThreads = DNSProcessThreads.Count > 0;

            bool haveJobs = false;
            lock (PendingDNSLookups)
                haveJobs = PendingDNSLookups.Count > 0;

            if (haveJobs && !haveThreads)
            {
                lock (DNSProcessThreads)
                    StartDNSLookupThread();
            }
        }

        protected void PushDNSLookup(Peer peer)
        {
            DNSLookupJob job = new DNSLookupJob();
            job.PeerID = peer.ID;
            job.Address = peer.SocketConnection.RemoteEndPoint.Address;

            lock (PendingDNSLookups)
                PendingDNSLookups.Add(job);

            lock (DNSProcessThreads)
            {
                if (DNSProcessThreads.Count < MaxDNSThreds)
                    StartDNSLookupThread();
            }
        }

        protected void StartDNSLookupThread()
        {
            Thread t = new Thread(new ThreadStart(ProcessDNSJob));
            t.Start();
            DNSProcessThreads.Add(t);
        }

        protected void ProcessDNSJob()
        {
            // pull a job off the stack
            DNSLookupJob myJob = null;
            lock (PendingDNSLookups)
            {
                if (PendingDNSLookups.Count > 0)
                {
                    myJob = PendingDNSLookups[0];
                    PendingDNSLookups.RemoveAt(0);
                }
            }

            while (myJob != null)   // while we have work
            {
                myJob.HostInfo = System.Net.Dns.GetHostEntry(myJob.Address);    // look up our data

                lock (CompletedDNSLookups)          // push that sucker to the finished stack
                    CompletedDNSLookups.Add(myJob);

                myJob = null;

                lock (PendingDNSLookups)    // get the next job
                {
                    if (PendingDNSLookups.Count > 0)
                    {
                        myJob = PendingDNSLookups[0];
                        PendingDNSLookups.RemoveAt(0);
                    }
                }
            }

            lock(DNSProcessThreads) // we are done, remove ourselves
            {
                DNSProcessThreads.Remove(Thread.CurrentThread);
            }
        }
    }
}
