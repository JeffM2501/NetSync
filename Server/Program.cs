using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Server.Host;

namespace Server
{
    class Program
    {
        private static bool Done = false;

        private static object ExitLocker = new object();

        public static void Exit()
        {
            lock (ExitLocker)
                Done = true;
        }

        static void Main(string[] args)
        {
            ServerHost gameHost = new ServerHost();
            gameHost.Listen(2501);

            while(true)
            {
                lock (ExitLocker)
                {
                    if (Done)
                        break;
                }
                   
                gameHost.ProcessSockets();
                Thread.Sleep(10);
            }

            gameHost.Shutdown();
        }
    }
}
