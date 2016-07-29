using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Server.Host;

namespace Server
{
    public class Program
    {
        private static bool Done = false;

        public class ServerLogEventArgs : EventArgs
        {
            public string Text = string.Empty;
            public ServerLogEventArgs(string t) { Text = t; }
        }

        public static event EventHandler<ServerLogEventArgs> LogTextGenerated = null;

        private static object ExitLocker = new object();

        public static void Exit()
        {
            lock (ExitLocker)
                Done = true;
        }

        public static int Port = 2501;

        public static void Main(string[] args)
        {
            ServerHost gameHost = new ServerHost();
            gameHost.Listen(Port);

            while(true)
            {
                lock (ExitLocker)
                {
                    if (Done)
                        break;
                }
                   
                gameHost.ProcessSockets();

				var logLine = gameHost.GetLogLine();
				if(logLine != string.Empty)
                {
                    if (LogTextGenerated != null)
                        LogTextGenerated.Invoke(gameHost, new ServerLogEventArgs(logLine));
                    else
                        Console.WriteLine(logLine);
                }
					
                Thread.Sleep(10);
            }

            gameHost.Shutdown();
        }
    }
}
