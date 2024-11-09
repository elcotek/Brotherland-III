using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using log4net;
using Timer = System.Windows.Forms.Timer;

namespace DOL.GS.Scripts
{
    #region DebugPacketForm class

    public class DebugPacketForm : Form
    {
        private const string PACKET_CODE = "AF";
        private const string PACKET_CONTENT = "FF FF 00 5A 00 00 00 00 59 6F 75 20 61 72 65 20 72 65 61 64 79 20 74 6F 20 66 69 72 65 21 00";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private TextBox packetCode;
        private Label label1;
        private Label label2;
        private GameClient m_client;
        private Button sendButton;
        private RichTextBox contentTextBox;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private Label label3;
        private TextBox sessionID;
        private TextBox targetOID;
        private Label label4;
        private TextBox playerOID;
        private Label label5;
        private Button refreshButton;
        private System.Windows.Forms.Timer m_refreshTimer;
        private Label label6;
        private TextBox clientState;
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private Container components = null;

        public DebugPacketForm(GameClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            m_client = client;
            InitializeComponent();
            string name;
            if (client.Player == null)
                name = (client.Account == null ? client.GetType().ToString() : client.Account.Name);
            else name = client.Player.Name;
            Text = "Send Packet to " + name;
        }

        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code
        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.packetCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.contentTextBox = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.sendButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.refreshButton = new System.Windows.Forms.Button();
            this.playerOID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.targetOID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.sessionID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.clientState = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // packetCode
            // 
            this.packetCode.Location = new System.Drawing.Point(136, 8);
            this.packetCode.Name = "packetCode";
            this.packetCode.Size = new System.Drawing.Size(72, 20);
            this.packetCode.TabIndex = 0;
            this.packetCode.Text = "";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Packet Code (HEX Value)";
            // 
            // contentTextBox
            // 
            this.contentTextBox.Location = new System.Drawing.Point(0, 48);
            this.contentTextBox.Name = "contentTextBox";
            this.contentTextBox.Size = new System.Drawing.Size(248, 136);
            this.contentTextBox.TabIndex = 2;
            this.contentTextBox.Text = "";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(248, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Packet Content (HEX Values)";
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(0, 192);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(248, 23);
            this.sendButton.TabIndex = 4;
            this.sendButton.Text = "Send Packet";
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(256, 8);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(224, 208);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.refreshButton);
            this.tabPage1.Controls.Add(this.playerOID);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.targetOID);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.sessionID);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.clientState);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(216, 182);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Useful Information";
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(0, 160);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(216, 22);
            this.refreshButton.TabIndex = 6;
            this.refreshButton.Text = "Refresh Values";
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // playerOID
            // 
            this.playerOID.Location = new System.Drawing.Point(69, 31);
            this.playerOID.Name = "playerOID";
            this.playerOID.ReadOnly = true;
            this.playerOID.TabIndex = 5;
            this.playerOID.Text = "";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(7, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "TargetOID";
            // 
            // targetOID
            // 
            this.targetOID.Location = new System.Drawing.Point(69, 53);
            this.targetOID.Name = "targetOID";
            this.targetOID.ReadOnly = true;
            this.targetOID.TabIndex = 3;
            this.targetOID.Text = "";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Player OID";
            // 
            // sessionID
            // 
            this.sessionID.Location = new System.Drawing.Point(69, 9);
            this.sessionID.Name = "sessionID";
            this.sessionID.ReadOnly = true;
            this.sessionID.TabIndex = 1;
            this.sessionID.Text = "";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "SessionID";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(7, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 16);
            this.label6.TabIndex = 7;
            this.label6.Text = "ClientState";
            // 
            // clientState
            // 
            this.clientState.Location = new System.Drawing.Point(69, 75);
            this.clientState.Name = "clientState";
            this.clientState.ReadOnly = true;
            this.clientState.TabIndex = 3;
            this.clientState.Text = "";
            // 
            // DebugPacketForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(480, 221);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.contentTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.packetCode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "DebugPacketForm";
            this.Text = "Send Packet";
            this.Load += new System.EventHandler(this.DebugPacketForm_Load);
            this.Closed += new System.EventHandler(this.DebugPacketForm_Closed);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void DebugPacketForm_Load(object sender, EventArgs e)
        {

            m_refreshTimer = new System.Windows.Forms.Timer();
            m_refreshTimer.Interval = 500;
            m_refreshTimer.Tick += new EventHandler(RefreshTimerCallback);
            m_refreshTimer.Start();
            RefreshUsefulInformation();
            packetCode.Text = PACKET_CODE;
            contentTextBox.Text = PACKET_CONTENT;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                string text = contentTextBox.Text.Trim();
                string[] bytestrings = text.Split(new char[] { ',', ' ', ';' });
                string codestring = packetCode.Text.Trim();
                ArrayList bytes = new ArrayList();
                byte code = byte.Parse(codestring, NumberStyles.HexNumber);
                foreach (string bytestring in bytestrings)
                    if (bytestring.Trim().Length > 0)
                        bytes.Add(byte.Parse(bytestring.Trim(), NumberStyles.HexNumber));

                GSTCPPacketOut pak = new GSTCPPacketOut(code);
                foreach (byte b in bytes)
                    pak.WriteByte(b);

                m_client.Out.SendTCP(pak);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshTimerCallback(object sender, EventArgs e)
        {
            RefreshUsefulInformation();
        }

        private void RefreshUsefulInformation()
        {
            playerOID.Text = String.Format("{0}", (m_client.Player != null) ? m_client.Player.ObjectID.ToString("X4") : "(null)");
            sessionID.Text = String.Format("{0:X4}", m_client.SessionID);
            clientState.Text = m_client.ClientState.ToString();
            if (m_client.Player != null && m_client.Player.TargetObject != null)
                targetOID.Text = String.Format("{0:X4}", m_client.Player.TargetObject.ObjectID);
            else
                targetOID.Text = "";
        }

        private void refreshButton_Click(object sender, System.EventArgs e)
        {
            RefreshUsefulInformation();
        }

        private void DebugPacketForm_Closed(object sender, System.EventArgs e)
        {
            m_refreshTimer.Stop();
            m_refreshTimer.Dispose();
        }
    }
}
#endregion

namespace DOL.GS.Commands
{
    [Cmd(
        "&custompacket",
           ePrivLevel.Admin,
          "Send custom packets to clients",
          "/custompacket [name] [a] - dialog for current client or find by player or account name")]
    public class CustomPacketCommand : AbstractCommandHandler, ICommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private class ShowDialog
        {
            private Form m_dialog;
            public ShowDialog(Form dialog)
            {
                m_dialog = dialog;
                new Thread(new ThreadStart(Proc)).Start();
            }
            private void Proc()
            {
                try
                {
                    m_dialog.ShowDialog();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /*
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length == 1)
            {
                DisplaySyntax(client);
                return;
            }
            string code = String.Join(" ", args, 1, args.Length - 1);
            ExecuteCode(client, code);
        }
        */

        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length == 1)
            {
                DisplaySyntax(client);
                return;
            }


            GameClient dialogClient = client;

            try
            {
                bool findAccount = args.Length > 2 && args[2].ToLower() == "a";
                string searchType = findAccount ? "account" : "player";
                if (args.Length > 1)
                {
                    if (findAccount)
                        dialogClient = WorldMgr.GetClientByAccountName(args[1], false);
                    else
                        dialogClient = WorldMgr.GetClientByPlayerName(args[1], false, true);

                    if (dialogClient == null)
                    {
                        client.Out.SendMessage(string.Format("Client with {0} name {1} not found.", searchType, args[1]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }

                    string name = findAccount ? dialogClient.Account.Name : dialogClient.Player.Name;
                    client.Out.SendMessage(string.Format("Dialog for {0} '{1}'", searchType, name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                new ShowDialog(new DebugPacketForm(dialogClient));
            }
            catch (Exception e)
            {
                client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
            }

            return;
        }
    }
}
