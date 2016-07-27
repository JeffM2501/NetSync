using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Server.Host;

namespace Server.Security
{
    public class DNSLookupProcessor
    {
        public class DNSLookupJob
        {
            public long PeerID = long.MaxValue;
            public IPAddress Address = null;

            public IPHostEntry HostInfo = null;
        }

        private List<DNSLookupJob> PendingLookups = new List<DNSLookupJob>();
        private List<DNSLookupJob> CompletedLookups = new List<DNSLookupJob>();

        public static int MaxThreads = 2;

        private List<Thread> ProcessThreads = new List<Thread>();

        public void Shutdown()
        {
            lock (ProcessThreads)
            {
                foreach (var t in ProcessThreads)
                    t.Abort();

                ProcessThreads.Clear();
            }

            lock (PendingLookups)
                PendingLookups.Clear();

            lock (CompletedLookups)
                CompletedLookups.Clear();
        }

        public void RemoveAllJobsForPeer(Peer peer)
        {
            lock (PendingLookups)
                PendingLookups.RemoveAll(x => x.PeerID == peer.ID);
        }

        public DNSLookupJob PopCompletedJob()
        {
            DNSLookupJob job = null;
            lock (CompletedLookups)
            {
                if (CompletedLookups.Count == 0)
                    return null;

                job = CompletedLookups[0];
                CompletedLookups.RemoveAt(0);
            }

            return job;
        }

        public void CheckForDeadJobs()
        {
            bool haveThreads = false;
            lock (ProcessThreads)
                haveThreads = ProcessThreads.Count > 0;

            bool haveJobs = false;
            lock (PendingLookups)
                haveJobs = PendingLookups.Count > 0;

            if (haveJobs && !haveThreads)
            {
                lock (ProcessThreads)
                    StartLookupThread();
            }
        }

        public void PushDNSLookup(Peer peer)
        {
            DNSLookupJob job = new DNSLookupJob();
            job.PeerID = peer.ID;
            job.Address = peer.SocketConnection.RemoteEndPoint.Address;

            lock (PendingLookups)
                PendingLookups.Add(job);

            lock (ProcessThreads)
            {
                if (ProcessThreads.Count < MaxThreads)
                    StartLookupThread();
            }
        }

        protected void StartLookupThread()
        {
            Thread t = new Thread(new ThreadStart(ProcessJob));
            t.Start();
            ProcessThreads.Add(t);
        }

        protected void ProcessJob()
        {
            // pull a job off the stack
            DNSLookupJob myJob = null;
            lock (PendingLookups)
            {
                if (PendingLookups.Count > 0)
                {
                    myJob = PendingLookups[0];
                    PendingLookups.RemoveAt(0);
                }
            }

            while (myJob != null)   // while we have work
            {
                // do the job
                myJob.HostInfo = System.Net.Dns.GetHostEntry(myJob.Address);    // look up our data

                lock (CompletedLookups)          // push that sucker to the finished stack
                    CompletedLookups.Add(myJob);

                myJob = null;

                lock (PendingLookups)    // get the next job
                {
                    if (PendingLookups.Count > 0)
                    {
                        myJob = PendingLookups[0];
                        PendingLookups.RemoveAt(0);
                    }
                }
            }

            lock (ProcessThreads) // we are done, remove ourselves
                ProcessThreads.Remove(Thread.CurrentThread);
        }

    }
}
