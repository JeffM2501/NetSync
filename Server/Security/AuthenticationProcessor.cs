using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Server.Host;

namespace Server.Security
{
    public class AuthenticationProcessor
    {
        public class AuthenticationJob
        {
            public string URL = string.Empty;

            public string UserID = string.Empty;
            public string UserToken = string.Empty;

            public long PeerID = long.MinValue;

            public string Results = string.Empty;
        }

        private List<AuthenticationJob> PendingLookups = new List<AuthenticationJob>();
        private List<AuthenticationJob> CompletedLookups = new List<AuthenticationJob>();

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

        public AuthenticationJob PopCompletedJob()
        {
            AuthenticationJob job = null;
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

        public void PushAuthLookup(Peer peer, string host, string userID, string token)
        {
            AuthenticationJob job = new AuthenticationJob();
            job.PeerID = peer.ID;
            job.URL = "https://" + host;
            job.UserID = userID;
            job.UserToken = token;

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
            AuthenticationJob myJob = null;
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

                // for now just accept it; TODO call the auth system


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
