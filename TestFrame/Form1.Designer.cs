namespace TestFrame
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ServerLogText = new System.Windows.Forms.TextBox();
            this.ClientLogOutput = new System.Windows.Forms.TextBox();
            this.ClientConnect = new System.Windows.Forms.Button();
            this.ClientConnetionAddress = new System.Windows.Forms.TextBox();
            this.TickTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(284, 261);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.ClientConnetionAddress);
            this.tabPage1.Controls.Add(this.ClientConnect);
            this.tabPage1.Controls.Add(this.ClientLogOutput);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(276, 235);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Client";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.ServerLogText);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(276, 235);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Server";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ServerLogText
            // 
            this.ServerLogText.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ServerLogText.Location = new System.Drawing.Point(3, 160);
            this.ServerLogText.Multiline = true;
            this.ServerLogText.Name = "ServerLogText";
            this.ServerLogText.ReadOnly = true;
            this.ServerLogText.Size = new System.Drawing.Size(270, 72);
            this.ServerLogText.TabIndex = 0;
            // 
            // ClientLogOutput
            // 
            this.ClientLogOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ClientLogOutput.Location = new System.Drawing.Point(3, 164);
            this.ClientLogOutput.Multiline = true;
            this.ClientLogOutput.Name = "ClientLogOutput";
            this.ClientLogOutput.Size = new System.Drawing.Size(270, 68);
            this.ClientLogOutput.TabIndex = 0;
            // 
            // ClientConnect
            // 
            this.ClientConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ClientConnect.Location = new System.Drawing.Point(195, 6);
            this.ClientConnect.Name = "ClientConnect";
            this.ClientConnect.Size = new System.Drawing.Size(75, 23);
            this.ClientConnect.TabIndex = 1;
            this.ClientConnect.Text = "Connect";
            this.ClientConnect.UseVisualStyleBackColor = true;
            this.ClientConnect.Click += new System.EventHandler(this.ClientConnect_Click);
            // 
            // ClientConnetionAddress
            // 
            this.ClientConnetionAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ClientConnetionAddress.Location = new System.Drawing.Point(3, 8);
            this.ClientConnetionAddress.Name = "ClientConnetionAddress";
            this.ClientConnetionAddress.Size = new System.Drawing.Size(186, 20);
            this.ClientConnetionAddress.TabIndex = 2;
            this.ClientConnetionAddress.Text = "localhost";
            // 
            // TickTimer
            // 
            this.TickTimer.Interval = 50;
            this.TickTimer.Tick += new System.EventHandler(this.TickTimer_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox ServerLogText;
        private System.Windows.Forms.TextBox ClientLogOutput;
        private System.Windows.Forms.Button ClientConnect;
        private System.Windows.Forms.TextBox ClientConnetionAddress;
        private System.Windows.Forms.Timer TickTimer;
    }
}

