using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ClientLib;

namespace TestFrame
{
    public partial class Form1 : Form
    {
        protected Thread ServerThread = null;

        protected Client GameClient = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServerThread = new Thread(new ThreadStart(RunServer));
            ServerThread.Start();
        }

        void RunServer()
        {
            Server.Program.LogTextGenerated += Program_LogTextGenerated;
            Server.Program.Main(new List<string>().ToArray());
        }

        private void Program_LogTextGenerated(object sender, Server.Program.ServerLogEventArgs e)
        {
            ServerLogText.Text = e.Text + "\r\n" + ServerLogText.Text;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            GameClient = null;
            TickTimer.Stop();

            Server.Program.Exit();
            ServerThread = null;
        }

        private void ClientConnect_Click(object sender, EventArgs e)
        {
            GameClient = new Client();
            GameClient.Connect(ClientConnetionAddress.Text, Server.Program.Port, true);
            GameClient.HostConnected += GameClient_HostConnected;
            GameClient.HostDisconnected += GameClient_HostDisconnected;
            GameClient.GetAuthenticationCredentials += GameClient_GetAuthenticationCredentials;
            TickTimer.Start();
        }

        private void GameClient_GetAuthenticationCredentials(object sender, Client.AuthenticationCredentialsEventArgs e)
        {
            lock (ClientLogLines)
                ClientLogLines.Add("Credentials");

            e.UserID = "12";
            e.Token = Thread.CurrentThread.Name;
            e.TokenSource = Thread.CurrentThread.Name;

            e.SendNow = true;
        }

        protected List<string> ClientLogLines = new List<string>();

        private void GameClient_HostDisconnected(object sender, EventArgs e)
        {
            lock (ClientLogLines)
                ClientLogLines.Add("Disconnected");
            GameClient.Shutdown();
            GameClient = null;
            TickTimer.Stop();
        }

        private void GameClient_HostConnected(object sender, EventArgs e)
        {
            lock (ClientLogLines)
                ClientLogLines.Add("Connected");
        }

        private void TickTimer_Tick(object sender, EventArgs e)
        {
            if (GameClient != null)
                GameClient.Update();

            lock (ClientLogLines)
            {
                foreach (var c in ClientLogLines)
                    ClientLogOutput.Text = c;

                ClientLogLines.Clear();
            }
        }
    }
}
