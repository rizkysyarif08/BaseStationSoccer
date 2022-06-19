using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.NetworkInformation;
using NetworksApi.TCP.SERVER;
using System.Net;
using System.Net.Sockets;

namespace ServerNew_beta1
{
    public partial class Form1 : Form
    {
        Thread secondTask;
        TcpClient client;
        Server BaseServer;
        NetworkStream ns;
        IPEndPoint ipend;
        Ping ping = new Ping();
        PingReply reply;
        string[] receive = new string[27];

        //string ipRef = "192.168.1.103";
        string ipRef = "127.0.0.1";
        string myIP;
        string hostName;
        string client1IP, client2IP, client3IP;
        string client1Data, client2Data, client3Data;
        string dataBroadcast;
        string dataRef;
        string ref_com = "7";
        string keeperSend = "0";
        string keeper_com = "115";
        string challange = "000";

        string StringPostXR2="000";
        string StringPostYR2="000";
        string StringPostXR3="000";
        string StringPostYR3="000";

        char offSetyawK = '0';
        char offSetyawR2 = '0';
        char offSetyawR3 = '0';

        int dataAscii = 83;
        int z = 0;
        int yawR1 = 0;
        int inputR2 = 0;
        int calibR2 = 0;
        int coorXR2 = 0;
        int coorYR2 = 0;
        int postXR2 = 0;
        int postYR2 = 0;
        int yawR2 = 0;
        int hitballR2 = 0;
        int inputR3 = 0;
        int calibR3 = 0;
        int coorXR3 = 0;
        int coorYR3 = 0;
        int postXR3 = 0;
        int postYR3 = 0;
        int yawR3 = 0;
        int hitballR3 = 0;
        int postXR1 = 0;
        int postYR1 = 0;
        int coorXR1;
        int coorYR1;
        int calibR1;
        int mode = 1;
        int coorX, coorY;
        int ballDist2, ballAngel2;
        int ballDist3, ballAngel3;
        int colorTieam = 1;

        int kepeerMovement = 0;
        int forceCalib = 0;

        long timePingRef;
        long timePingR1;
        long timePingR2;
        long timePingR3;

        bool statPingRef = false;
        bool statPingR1 = false;
        bool statPingR2 = false;
        bool statPingR3 = false;

        bool KipeerMode = true;
        bool saveComm = false;

        bool stop = true;
        bool RefCon = false;
        bool keeper = false;
        bool offsetK = false;
        bool offsetR2 = false;
        bool offsetR3 = false;
        bool backR2 = false;
        bool backR3 = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hostName = Dns.GetHostName();
            myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            this.Text = "Base Station " + myIP.ToString();

            //ipend = new IPEndPoint(IPAddress.Any, 0);

            secondTask = new Thread(new ThreadStart(performPING1));
            secondTask.Start();

            BaseServer = new Server(myIP, "3129");
            BaseServer.OnClientConnected += new OnConnectedDelegate(BaseServer_OnConnected);
            BaseServer.OnClientDisconnected += new OnDisconnectedDelegate(BaseServer_OnDisconnect);
            BaseServer.OnDataReceived += new OnReceivedDelegate(BaseServer_OnReceived);
            BaseServer.OnServerError += new OnErrorDelegate(BaseServer_OnServerError);
            BaseServer.Start();

        }
        
        private void performPING1()
        {
            while (true)
            {
                //Ping Client
                if (RefCon && ipRef != "127.0.0.1")
                {
                    reply = ping.Send(ipRef, 500);
                    if (reply != null)
                    {
                        timePingRef = reply.RoundtripTime;
                        statPingRef = true;
                        if (timePingRef > 500)
                        {
                            //client3IP = null;
                            statPingRef = false;
                        }
                    }
                }
                else { statPingRef = false; dataRef = null; }
                //Disconnect
                
                if (client1IP != null)
                {
                    reply = ping.Send(client1IP, 500);
                    if (reply != null)
                    {
                        timePingR1 = reply.RoundtripTime;
                        statPingR1 = true;
                        if (reply.RoundtripTime > 500)
                        {
                            client1IP = null;
                            statPingR1 = false;
                        }
                    }
                }
                else { statPingR1 = false; client1Data = null; }
                
                if (client2IP != null)
                {
                    reply = ping.Send(client2IP, 500);
                    if (reply != null)
                    {
                        timePingR2 = reply.RoundtripTime;
                        statPingR2 = true;
                        if (reply.RoundtripTime > 500)
                        {
                            client2IP = null;
                            statPingR2 = false;
                        }
                    }
                }
                else { statPingR2 = false; client2Data = null; }
                if (client3IP != null)
                {
                    reply = ping.Send(client3IP, 500);
                    if (reply != null)
                    {
                        timePingR3 = reply.RoundtripTime;
                        statPingR3 = true;
                        if (reply.RoundtripTime > 500)
                        {
                            client3IP = null;
                            statPingR3 = false;
                        }
                    }
                }
                else { statPingR3 = false; client3Data = null; }
            }
        }

        #region TCPConnection
        private void BaseServer_OnConnected(object sender, ConnectedArguments R)
        {
            if (R.Name == "Robot1")
            {
                lb_conR1.BackColor = Color.LimeGreen;
                client1IP = R.Ip;
            }
            if (R.Name == "Robot2")
            {
                lb_conR2.BackColor = Color.LimeGreen;
                client2IP = R.Ip;
            }
            if (R.Name == "Robot3")
            {
                lb_conR3.BackColor = Color.LimeGreen;
                client3IP = R.Ip;
            }
        }

        private void BaseServer_OnDisconnect(object sender, DisconnectedArguments R)
        {
            if (R.Name == "Robot1")
            {
                lb_conR1.BackColor = Color.IndianRed;
                //lb_timeoutR1.Text = "TimeOut";
                lb_timeoutR1.BackColor = Color.IndianRed;
                client1Data = null;
            }
            if (R.Name == "Robot2")
            {
                lb_conR2.BackColor = Color.IndianRed;
                //lb_timeoutR2.Text = "TimeOut";
                lb_timeoutR2.BackColor = Color.IndianRed;
                client2Data = null;
                hitballR2 = 0;
                postXR2 = 0;
                postYR2 = 0;
            }
            if (R.Name == "Robot3")
            {
                lb_conR3.BackColor = Color.IndianRed;
                //lb_timeoutR3.Text = "TimeOut";
                lb_timeoutR3.BackColor = Color.IndianRed;
                client3Data = null;
                hitballR3 = 0;
                postYR2 = 0;
                postYR3 = 0;
            }
        }

        private void BaseServer_OnReceived(object sender, ReceivedArguments R)
        {
            if (R.Name == "Robot1")
            {
                client1Data = R.ReceivedData;
            }
            if (R.Name == "Robot2")
            {
                client2Data = R.ReceivedData;
            }
            if (R.Name == "Robot3")
            {
                client3Data = R.ReceivedData;
            }
        }

        private void BaseServer_OnServerError(object sender, ErrorArguments R)
        {

        }

        #endregion

        #region Bottom
        private void bt_keeper_Click(object sender, EventArgs e)
        {
            if (keeper)
            {
                keeper = false;
                bt_keeper.BackColor = Color.DarkRed;
                kepeerMovement = 5;
            }
            else
            {
                keeper = true;
                bt_keeper.BackColor = Color.ForestGreen;
                kepeerMovement = 0;
            }
        }

        private void bt_connect_Click(object sender, EventArgs e)
        {
            if (RefCon)
            {
                RefCon = false;
                client.Close();
                if(!client.Connected) bt_connect.BackColor = Color.DarkRed;
                dataAscii = 115;
            }
            else
            {
                try
                {
                    client = new TcpClient(ipRef, 28097);
                    ns = client.GetStream();
                    if (client.Connected)
                    {
                        z = 100;
                        RefCon = true;
                        bt_connect.BackColor = Color.ForestGreen;
                        //timer1.Enabled = true;
                    }
                    else
                    {
                        RefCon = false;
                        bt_connect.BackColor = Color.DarkRed;
                        //timer1.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("RefBox Failed to Connect!!!");
                }
            }   
        }
        private void bt_offsetK_Click(object sender, EventArgs e)
        {
            if (offsetK)
            {
                offsetK = false;
                offSetyawK = '0';
                bt_offsetK.BackColor = Color.ForestGreen;
            }
            else
            {
                offsetK = true;
                offSetyawK = '1';
                bt_offsetK.BackColor = Color.DarkRed;
            }
        }

        private void bt_offsetR2_Click(object sender, EventArgs e)
        {
            if (offsetR2)
            {
                offsetR2 = false;
                offSetyawR2 = '0';
                bt_offsetR2.BackColor = Color.ForestGreen;
            }
            else
            {
                offsetR2 = true;
                offSetyawR2 = '1';
                bt_offsetR2.BackColor = Color.DarkRed;
            }
        }

        private void bt_offsetR3_Click(object sender, EventArgs e)
        {
            if (offsetR3)
            {
                offsetR3 = false;
                offSetyawR3 = '0';
                bt_offsetR3.BackColor = Color.ForestGreen;
            }
            else
            {
                offsetR3 = true;
                offSetyawR3 = '1';
                bt_offsetR3.BackColor = Color.DarkRed;
            }
        }

        private void bt_inputR2_Click(object sender, EventArgs e)
        {
            if (coorX != 0 && coorY != 0 || inputR2 == 1)
            {
                if (inputR2 == 0)
                {
                    coorXR2 = coorX;
                    coorYR2 = coorY;

                    coorX = 0;
                    coorY = 0;

                    inputR2 = 1;
                    bt_inputR2.BackColor = Color.IndianRed;
                }
                else
                {
                    inputR2 = 0;
                    bt_inputR2.BackColor = Color.LimeGreen;

                    coorXR2 = 0;
                    coorYR2 = 0;
                }
            }
            if (calibR2 == 1)
            {
                bt_calibR2_Click(null, null);
            }
        }
        private void bt_stop_Click(object sender, EventArgs e)
        {
            if (!stop)
            {
                stop = true;
            }
            bt_start.BackColor = Color.ForestGreen;
            bt_stop.BackColor = Color.DarkRed;
            bt_kickoff.BackColor = Color.ForestGreen;
            bt_freekick.BackColor = Color.ForestGreen;
            bt_dropball.BackColor = Color.ForestGreen;
            bt_goalkick.BackColor = Color.ForestGreen;
            bt_emgst.BackColor = Color.ForestGreen;
            bt_corner.BackColor = Color.ForestGreen;
            bt_penalty.BackColor = Color.ForestGreen;
            ref_com = "7";

            inputR2 = 0;
            bt_inputR2.BackColor = Color.LimeGreen;

            inputR3 = 0;
            bt_inputR3.BackColor = Color.LimeGreen;
        }

        private void bt_start_Click(object sender, EventArgs e)
        {
            if (inputR2 == 1) bt_inputR2_Click(null, null);
            if (inputR3 == 1) bt_inputR3_Click(null, null);
            if (calibR2 == 1) bt_calibR2_Click(null, null);
            if (calibR3 == 1) bt_calibR3_Click(null, null);
   
            if (stop || saveComm)
            {
                stop = false;
                saveComm = false;
                bt_start.BackColor = Color.DarkRed;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.ForestGreen;
                //ref_com = "115"; //s
                ref_com = "8";               
            }
            if (!KipeerMode) bt_modeR1_Click(null, null);
        }

        private void bt_kickoff_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                bt_start.BackColor = Color.ForestGreen;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.DarkRed;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.ForestGreen;
                //ref_com = "75"; //K
                ref_com = "1";
            }
        }

        private void bt_freekick_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                bt_start.BackColor = Color.ForestGreen;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.DarkRed;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.ForestGreen;
                //ref_com = "70"; //F 
                ref_com = "2";
            }
        }

        private void bt_dropball_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                bt_start.BackColor = Color.ForestGreen;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.DarkRed;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.ForestGreen;
                //ref_com = "78"; //N
                ref_com = "9";
            }
        }

        private void bt_goalkick_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                bt_start.BackColor = Color.ForestGreen;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.DarkRed;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.ForestGreen;
                //ref_com = "71"; //G
                ref_com = "3";
            }
        }

        private void bt_throwin_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                bt_start.BackColor = Color.ForestGreen;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.DarkRed;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.ForestGreen;
                //ref_com = "115"; //T
                ref_com = "4";
            }
        }

        private void bt_corner_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                bt_start.BackColor = Color.ForestGreen;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.DarkRed;
                bt_penalty.BackColor = Color.ForestGreen;
                //ref_com = "67"; //C
                ref_com = "5";
            }
        }

        private void bt_penalty_Click(object sender, EventArgs e)
        {
            if (stop)
            {
                bt_start.BackColor = Color.ForestGreen;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.DarkRed;
                //ref_com = "80"; //P
                ref_com = "6";
            }
        }

        private void bt_init_Click(object sender, EventArgs e)
        {
            if (inputR2 == 0)
            {
                backR2 = true;
                coorXR2 = -25;
                coorYR2 = 100;

                inputR2 = 1;
                bt_inputR2.BackColor = Color.IndianRed;
            }
            else
            {
                inputR2 = 0;
                bt_inputR2.BackColor = Color.LimeGreen;
            }

            if (inputR3 == 0)
            {
                backR3 = true;
                coorXR3 = -25;
                coorYR3 = 500;

                inputR3 = 1;
                bt_inputR3.BackColor = Color.IndianRed;
            }
            else
            {
                inputR3 = 0;
                bt_inputR3.BackColor = Color.LimeGreen;
            }
        }

        private void bt_releaseR2_Click(object sender, EventArgs e)
        {
            if (forceCalib == 0 || forceCalib == 2)
            {
                forceCalib = 1;
                bt_releaseR2.BackColor = Color.LimeGreen;
            }
            else
            {
                forceCalib = 0;
                bt_releaseR2.BackColor = Color.DarkRed;
            }
        }

        private void bt_releaseR3_Click(object sender, EventArgs e)
        {
            if (forceCalib == 0 || forceCalib == 1)
            {
                forceCalib = 2;
                bt_releaseR3.BackColor = Color.LimeGreen;
            }
            else
            {
                forceCalib = 0;
                bt_releaseR3.BackColor = Color.DarkRed;
            }
        }

        private void bt_modeR1_Click(object sender, EventArgs e)
        {
            if (KipeerMode)
            {
                KipeerMode = false;
                bt_modeR1.BackColor = Color.DarkRed;
                bt_modeR1.Text = "Manual";
                bt_keeper.Enabled = true;
                keeper_com = "83";
            }
            else
            {
                KipeerMode = true;
                bt_modeR1.BackColor = Color.DarkGreen;
                bt_modeR1.Text = "Auto";
                bt_keeper.Enabled = false;
                kepeerMovement = 0;
                keeper_com = "115";
            }
        }

        private void bt_inputR3_Click(object sender, EventArgs e)
        {
            if (coorX != 0 && coorY != 0 || inputR3 == 1)
            {
                if (inputR3 == 0)
                {
                    coorXR3 = coorX;
                    coorYR3 = coorY;

                    coorX = 0;
                    coorY = 0;

                    inputR3 = 1;
                    bt_inputR3.BackColor = Color.IndianRed;
                }
                else
                {
                    inputR3 = 0;
                    bt_inputR3.BackColor = Color.LimeGreen;

                    coorXR3 = 0;
                    coorYR3 = 0;
                }
            }
            if (calibR3 == 1)
            {
                bt_calibR3_Click(null, null);
            }
        }

        private void bt_calibR2_Click(object sender, EventArgs e)
        {
            if (coorX != 0 && coorY != 0 || calibR2 == 1)
            {
                if (calibR2 == 0)
                {
                    coorXR2 = coorX;
                    coorYR2 = coorY;

                    coorX = 0;
                    coorY = 0;

                    calibR2 = 1;
                    bt_calibR2.BackColor = Color.IndianRed;
                }
                else
                {
                    calibR2 = 0;
                    bt_calibR2.BackColor = Color.LimeGreen;

                    coorXR2 = 0;
                    coorYR2 = 0;
                }
                if (inputR2 == 1)
                {
                    bt_inputR2_Click(null, null);
                }
            }
        }
        private void bt_calibR3_Click(object sender, EventArgs e)
        {
            if (coorX != 0 && coorY != 0 || calibR3 == 1)
            {
                if (calibR3 == 0)
                {
                    coorXR3 = coorX;
                    coorYR3 = coorY;

                    coorX = 0;
                    coorY = 0;

                    calibR3 = 1;
                    bt_calibR3.BackColor = Color.IndianRed;
                }
                else
                {
                    calibR3 = 0;
                    bt_calibR3.BackColor = Color.LimeGreen;

                    coorXR3 = 0;
                    coorYR3 = 0;
                }
            }
            if (inputR3 == 1)
            {
                bt_inputR3_Click(null, null);
            }
        }
        #endregion

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (!KipeerMode)
            {
                if (e.KeyCode == Keys.J) kepeerMovement = 0;
                if (e.KeyCode == Keys.I) kepeerMovement = 0;
                if (e.KeyCode == Keys.L) kepeerMovement = 0;
                if (e.KeyCode == Keys.K) kepeerMovement = 0;
            }
        }

        void save_comm()
        {
            if (!saveComm && stop)
            {
                saveComm = true;
                stop = false;
                ref_com = dataAscii.ToString();
                
                bt_start.BackColor = Color.Yellow;
                bt_stop.BackColor = Color.ForestGreen;
                bt_kickoff.BackColor = Color.ForestGreen;
                bt_freekick.BackColor = Color.ForestGreen;
                bt_dropball.BackColor = Color.ForestGreen;
                bt_goalkick.BackColor = Color.ForestGreen;
                bt_emgst.BackColor = Color.ForestGreen;
                bt_corner.BackColor = Color.ForestGreen;
                bt_penalty.BackColor = Color.ForestGreen;
            }
            else
            {
                saveComm = false;
                bt_start.BackColor = Color.DarkRed;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z) bt_start_Click(null, null);
            if (e.KeyCode == Keys.X) bt_stop_Click(null, null);

            if (e.KeyCode == Keys.E) bt_kickoff_Click(null, null);
            if (e.KeyCode == Keys.R) bt_freekick_Click(null, null);
            if (e.KeyCode == Keys.T) bt_goalkick_Click(null, null);
            if (e.KeyCode == Keys.Y) bt_corner_Click(null, null);
            if (e.KeyCode == Keys.P) bt_init_Click(null, null);
            if (e.KeyCode == Keys.F) bt_dropball_Click(null, null);
            if (e.KeyCode == Keys.G) bt_penalty_Click(null, null);
            if (e.KeyCode == Keys.D) bt_throwin_Click(null, null);

            if (e.KeyCode == Keys.Q) bt_inputR2_Click(null, null);
            if (e.KeyCode == Keys.A) bt_calibR2_Click(null, null);
            if (e.KeyCode == Keys.W) bt_inputR3_Click(null, null);
            if (e.KeyCode == Keys.S) bt_calibR3_Click(null, null);

            if (e.KeyCode == Keys.Space) btnCM_Click(null, null);

            if (e.KeyCode == Keys.D1) rb_mode1.Checked = true;
            if (e.KeyCode == Keys.D2) rb_mode2.Checked = true;
            if (e.KeyCode == Keys.D3) rb_mode3.Checked = true;
            if (e.KeyCode == Keys.D4) rb_mode4.Checked = true;
            if (e.KeyCode == Keys.D5) rb_mode5.Checked = true;
            if (e.KeyCode == Keys.D6) rb_mode6.Checked = true;

            if (e.KeyCode == Keys.C) save_comm();

            if (e.KeyCode == Keys.End) bt_modeR1_Click(null, null);

            if (!KipeerMode)
            {
                if (e.KeyCode == Keys.Home) bt_keeper_Click(null, null);
                if (e.KeyCode == Keys.J) kepeerMovement = 1;
                if (e.KeyCode == Keys.I) kepeerMovement = 2;
                if (e.KeyCode == Keys.L) kepeerMovement = 3;
                if (e.KeyCode == Keys.K) kepeerMovement = 4;
            }
        }

        private void rectangleShape2_MouseClick(object sender, MouseEventArgs e)
        {
            if (rb_field1.Checked)
            {
                coorX = (int)(1.3333 * e.X);
                coorY = (int)(1.3333 * e.Y);
            }
            else if (rb_field2.Checked)
            {
                coorX = e.X;
                coorY = e.Y;
            }

            /*if (inputR2 == 1)
            {
                inputR2 = 0;
                bt_inputR2.BackColor = Color.LimeGreen;
            }
            if (inputR3 == 1)
            {
                inputR3 = 0;
                bt_inputR3.BackColor = Color.LimeGreen;
            }*/
        }

        private void btnCM_Click(object sender, EventArgs e)
        {
            if(colorTieam == 1)
            {
                colorTieam = 2;
                btnCM.BackColor = Color.Magenta;
                btnCM.Text = "MAGENTA"; 
            }
            else if (colorTieam == 2)
            {
                colorTieam = 1;
                btnCM.BackColor = Color.Cyan;
                btnCM.Text = "CYAN";
            }
        }

        private void rectangleShape1_MouseClick(object sender, MouseEventArgs e)
        {
            if (rb_field1.Checked)
            {
                coorX = (int)((1.3333 * e.X) - 50);
                coorY = (int)((1.3333 * e.Y) - 50);

            }
            else if (rb_field2.Checked)
            {
                coorX = (int)(e.X - 50);
                coorY = (int)(e.Y - 50);
            }
        }

        private void order(int data_input)
        {
            if (data_input == 83) ref_com = "7";
            else if (data_input == 115) ref_com = "8";
            else if (data_input == 78) ref_com = "9";
            else if (data_input == 87) ref_com = "9";

            else if (data_input == 75) ref_com = "1";
            else if (data_input == 70) ref_com = "2";
            else if (data_input == 71) ref_com = "3";
            else if (data_input == 84) ref_com = "4";
            else if (data_input == 67) ref_com = "5";
            else if (data_input == 80) ref_com = "6";

            else if (data_input == 107) ref_com = "11";
            else if (data_input == 102) ref_com = "12";
            else if (data_input == 103) ref_com = "13";
            else if (data_input == 116) ref_com = "14";
            else if (data_input == 99) ref_com = "15";
            else if (data_input == 112) ref_com = "16";

            //kickoff 75 freekick 70 drop 78 goal 71 throwin 115 corner 67 penalty 50
        }
        private void checking(object sender, EventArgs e)
        {
            bt_offsetK.Text = "YR1 " + yawR1.ToString();
            bt_offsetR2.Text = "YR2 " + yawR2.ToString();
            bt_offsetR3.Text = "YR3 " + yawR3.ToString();
            
            //YawStatus
            if (offsetK)
            {
                if (yawR1 == 0)
                {
                    offSetyawK = '0';
                    offsetK = false;
                    bt_offsetK.BackColor = Color.ForestGreen;
                }
            }

            if (offsetR2)
            {
                if (yawR2 == 0)
                {
                    offSetyawR2 = '0';
                    offsetR2 = false;
                    bt_offsetR2.BackColor = Color.ForestGreen;
                }
            }

            if (offsetR3)
            {
                if (yawR3 == 0)
                {
                    offSetyawR3 = '0';
                    offsetR3 = false;
                    bt_offsetR3.BackColor = Color.ForestGreen;
                }
            }

            //TCP Server
            if (RefCon)
            {
                bt_connect.BackColor = Color.ForestGreen;
            }
            else
            {
                bt_connect.BackColor = Color.IndianRed;
            }

            //Mode On Off
            if (rb_mode1.Checked == true) mode = 1;
            else if (rb_mode2.Checked == true) mode = 2;
            else if (rb_mode3.Checked == true) mode = 3;
            else if (rb_mode4.Checked == true) mode = 4;
            else if (rb_mode5.Checked == true) mode = 5;
            else if (rb_mode6.Checked == true) mode = 6;

            //Emergency Stop
            if (!stop && RefCon && !saveComm)
            {
                if (lb_recRef.Text != "1s") order(dataAscii);
                else ref_com = "8";
            }

            //Auto Off Calib SetInput
            /*if (backR2)
            {
                if (postXR2 == coorXR2 && postYR2 == coorYR2)
                {
                    inputR2 = 0;
                    bt_inputR2.BackColor = Color.LimeGreen;
                }
            }
            if (backR3)
            {
                if (postXR3 == coorXR3 && postYR2 == coorYR2)
                {
                    inputR3 = 0;
                    bt_inputR3.BackColor = Color.LimeGreen;
                }
            }*/
            if (calibR2 == 1)
            {
                if (postXR2 == coorXR2 && postYR2 == coorYR2)
                {
                    bt_calibR2_Click(null, null);
                }
            }

            if (calibR3 == 1)
            {
                if (postXR3 == coorXR3 && postYR3 == coorYR3)
                {
                    bt_calibR3_Click(null, null);
                }
            }

            //ReportPING
            lb_timeoutRef.Text = timePingRef.ToString();
            if (statPingRef)
            {
                lb_timeoutRef.BackColor = Color.LimeGreen;
                lb_conRef.BackColor = Color.LimeGreen;
            }
            else
            {
                lb_conRef.BackColor = Color.IndianRed;
                lb_timeoutRef.Text = "Timeout";
                lb_timeoutRef.BackColor = Color.IndianRed;
            }

            lb_timeoutR1.Text = timePingR1.ToString();
            if (statPingR1)
            {
                lb_timeoutR1.BackColor = Color.LimeGreen;
            }
            else
            {
                lb_timeoutR1.Text = "Timeout";
                lb_timeoutR1.BackColor = Color.IndianRed;
            }

            lb_timeoutR2.Text = timePingR2.ToString();
            if (statPingR2)
            {
                lb_timeoutR2.BackColor = Color.LimeGreen;
            }
            else
            {
                lb_timeoutR2.Text = "Timeout";
                lb_timeoutR2.BackColor = Color.IndianRed;
            }

            lb_timeoutR3.Text = timePingR3.ToString();
            if (statPingR3)
            {
                lb_timeoutR3.BackColor = Color.LimeGreen;
            }
            else
            {
                lb_timeoutR3.Text = "Timeout";
                lb_timeoutR3.BackColor = Color.IndianRed;
            }

            //Force Calib
            if (rb_field1.Checked)
            {
                if (forceCalib == 1 && postXR2 == -25 && postYR2 == 137)
                {
                    forceCalib = 0;
                    bt_releaseR2.BackColor = Color.DarkRed;
                }
                if (forceCalib == 2 && postXR3 == -25 && postYR3 == 662)
                {
                    forceCalib = 0;
                    bt_releaseR3.BackColor = Color.DarkRed;
                }
            }
            else if (rb_field2.Checked)
            {
                if (forceCalib == 1 && postXR2 == -25 && postYR2 == 100)
                {
                    forceCalib = 0;
                    bt_releaseR2.BackColor = Color.DarkRed;
                }
                if (forceCalib == 2 && postXR3 == -25 && postYR3 == 500)
                {
                    forceCalib = 0;
                    bt_releaseR3.BackColor = Color.DarkRed;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            checking(null,null);

            hitballR2 = 0;
            hitballR3 = 0;

            #region RefBoxDataReceive
            //Refbox Data Received
            if (RefCon)
            {
                bt_connect.BackColor = Color.ForestGreen;
                byte[] buffer = new byte[10];
                if (ns.DataAvailable)
                {
                    int Avaliable = ns.Read(buffer, 0, buffer.Length);
                    dataAscii = Convert.ToInt32(buffer[0]);
                    dataRef = Encoding.ASCII.GetString(buffer);
                    lb_recRef.Text = dataRef;
                    if (dataRef != null && dataAscii != 115)
                    {
                        if (char.IsLower(dataRef[0]))
                        {
                            ov_Robot2.BackColor = Color.Magenta;
                            ov_Robot3.BackColor = Color.Magenta;
                            lb_noRobot2.BackColor = Color.Magenta;
                            lb_noRobot3.BackColor = Color.Magenta;
                        }
                        else if (char.IsUpper(dataRef[0]))
                        {
                            ov_Robot2.BackColor = Color.Cyan;
                            ov_Robot3.BackColor = Color.Cyan;
                            lb_noRobot2.BackColor = Color.Cyan;
                            lb_noRobot3.BackColor = Color.Cyan;
                        }
                    }
                }
            }
            else
            {
                bt_connect.BackColor = Color.DarkRed;
            }
            #endregion

            #region ClientDataReceive
            //Client Robot Data Received
            /*if (client1Data != null)
            {
                tb_recRobot1.Text = client1Data;
                try
                {
                    receive = client1Data.Split('#');               
                    yawR1 = Convert.ToInt32(receive[2]);
                    postXR1 = Convert.ToInt32(receive[5]);
                    postYR1 = Convert.ToInt32(receive[4]);
                }
                catch(Exception ex) { }
            }
            else
            {
                tb_recRobot1.Text = "NULL";
            }*/
            if (client2Data != null)
            {
                tb_recRobot2.Text = client2Data;
                try
                {
                    receive = client2Data.Split('#');
                    yawR2 = Convert.ToInt32(receive[2]);
                    hitballR2 = Convert.ToInt32(receive[5]);
                    postXR2 = Convert.ToInt32(receive[7]);
                    postYR2 = Convert.ToInt32(receive[6]);
                    ballDist2 = Convert.ToInt32(receive[3]);
                    ballAngel2 = Convert.ToInt32(receive[4]);

                }
                catch (Exception ex) { }
            }
            else
            {
                tb_recRobot2.Text = "NULL";
            }
            
            if (client3Data != null)
            {
                tb_recRobot3.Text = client3Data;
                try
                {
                    receive = client3Data.Split('#');
                    yawR3 = Convert.ToInt32(receive[2]);
                    hitballR3 = Convert.ToInt32(receive[5]);
                    postXR3 = Convert.ToInt32(receive[7]);
                    postYR3 = Convert.ToInt32(receive[6]);
                    ballDist3 = Convert.ToInt32(receive[3]);
                    ballAngel3 = Convert.ToInt32(receive[4]);
                }
                catch (Exception ex) { }
            }
            else
            {
                tb_recRobot3.Text = "NULL";
            }
            #endregion

            //Post int to String 4digit
            if(postXR2 !=0  && postYR2 != 0)
            {
                if(postXR2 < 10)  StringPostXR2 = "000" + postXR2.ToString(); 
                else if(postXR2 < 100)  StringPostXR2 = "00" + postXR2.ToString(); 
                else if (postXR2 < 1000)  StringPostXR2 = "0" + postXR2.ToString(); 

                if (postYR2 < 10)  StringPostYR2 = "000" + postYR2.ToString(); 
                else if (postYR2 < 100) StringPostYR2 = "00" + postYR2.ToString(); 
                else if (postYR2 < 1000) StringPostYR2 = "0" + postYR2.ToString(); 
            }

            if(postXR3 !=0 && postYR3 != 0)
            {
                if (postXR3 < 10) StringPostXR3 = "000" + postXR3.ToString(); 
                else if (postXR3 < 100) StringPostXR3 = "00" + postXR3.ToString(); 
                else if (postXR3 < 1000) StringPostXR3 = "0" + postXR3.ToString(); 

                if (postYR3 < 10) StringPostYR3 = "000" + postYR3.ToString(); 
                else if (postYR3 < 100) StringPostYR3 = "00" + postYR3.ToString(); 
                else if (postYR3 < 1000) StringPostYR3 = "0" + postYR3.ToString(); 
            }

            #region RobotPosition
            //Robot and Yaw Field Position
            if (rb_field2.Checked)
            {
                ov_Robot2.Location = new Point(25 + postXR2, 25 + postYR2);
                ov_Robot3.Location = new Point(25 + postXR3, 25 + postYR3);
                ov_Robot1.Location = new Point(25 + postXR1, 25 + postYR1);

                //C left Size 112 455; Location 50 123
                rectangleShape4.Location = new Point(50, 123);
                rectangleShape4.Size = new Size(112, 445);

                //D left Size 38 305; Location 50 198
                rectangleShape3.Location = new Point(50, 198);
                rectangleShape3.Size = new Size(38, 305);

                //C right Size 112 455; Location 838 123
                rectangleShape5.Location = new Point(838, 123);
                rectangleShape5.Size = new Size(112, 455);
                //D right Size 38 305; Location 912 198
                rectangleShape6.Location = new Point(912, 198);
                rectangleShape6.Size = new Size(38, 305);
            }
            else if (rb_field1.Checked)
            {
                ov_Robot2.Location = new Point(25 + (int)(postXR2 * 0.75), 25 + (int)(postYR2 * 0.75));
                ov_Robot3.Location = new Point(25 + (int)(postXR3 * 0.75), 25 + (int)(postYR3 * 0.75));
                ov_Robot1.Location = new Point(25 + (int)(postXR1 * 0.75), 25 + (int)(postYR1 * 0.75));
                
                rectangleShape4.Location = new Point(50,(int)(142 * 0.75) + 50);
                rectangleShape4.Size = new Size((int)(180 * 0.75), (int)(525 * 0.75));
                
                rectangleShape3.Location = new Point(50, (int)(237 * 0.75) + 50);
                rectangleShape3.Size = new Size((int)(50 * 0.75), (int)(325 * 0.75));

                rectangleShape5.Location = new Point((int)(1020 * 0.75) + 50, (int)(142 * 0.75) + 50);
                rectangleShape5.Size = new Size((int)(180 * 0.75), (int)(525 * 0.75));

                rectangleShape6.Location = new Point((int)(1150 * 0.75) + 50, (int)(237 * 0.75) + 50);
                rectangleShape6.Size = new Size((int)(50 * 0.75), (int)(325 * 0.75));
            }

            ln_yawRobot1.Visible = false;
            lb_noRobot1.Visible = false;
            ov_Robot1.Visible = false;

            ln_yawRobot1.StartPoint = new Point(ov_Robot1.Location.X + 25, ov_Robot1.Location.Y + 25);
            int exR1 = ln_yawRobot1.StartPoint.X + (int)(Math.Cos(yawR1 * (Math.PI / 180.0)) * 25); //Deg to rad
            int eyR1 = ln_yawRobot1.StartPoint.Y + (int)(Math.Sin(yawR1 * (Math.PI / 180.0)) * 25);
            ln_yawRobot1.EndPoint = new Point(exR1, eyR1);
            lb_noRobot1.Location = new Point(ln_yawRobot1.StartPoint.X - 15, ln_yawRobot1.StartPoint.Y - 15);

            ln_yawRobot2.StartPoint = new Point(ov_Robot2.Location.X + 25, ov_Robot2.Location.Y + 25);
            int exR2 = ln_yawRobot2.StartPoint.X + (int)(Math.Cos(yawR2 * (Math.PI / 180.0)) * 25);
            int eyR2 = ln_yawRobot2.StartPoint.Y + (int)(Math.Sin(yawR2 * (Math.PI / 180.0)) * 25);
            ln_yawRobot2.EndPoint = new Point(exR2, eyR2);
            ln_degRobot2.StartPoint = ln_yawRobot2.StartPoint;
            lb_noRobot2.Location = new Point(ln_yawRobot2.StartPoint.X - 15, ln_yawRobot2.StartPoint.Y - 15);

            ln_yawRobot3.StartPoint = new Point(ov_Robot3.Location.X + 26, ov_Robot3.Location.Y + 26);
            int exR3 = ln_yawRobot3.StartPoint.X + (int)(Math.Cos(yawR3 * (Math.PI / 180.0)) * 25);
            int eyR3 = ln_yawRobot3.StartPoint.Y + (int)(Math.Sin(yawR3 * (Math.PI / 180.0)) * 25);
            ln_yawRobot3.EndPoint = new Point(exR3, eyR3);
            ln_degRobot3.StartPoint = ln_yawRobot3.StartPoint;
            lb_noRobot3.Location = new Point(ln_yawRobot3.StartPoint.X - 15, ln_yawRobot3.StartPoint.Y - 15);

            #endregion

            #region BallPosition
            //Ball Field Position
            int angleBallR2 = ((ballAngel2 - 90) - yawR2);
            if(angleBallR2 > 0)
            {
                angleBallR2 = 360 - angleBallR2;
            }
            else
            {
                angleBallR2 = Math.Abs(angleBallR2);
            }

            int angleBallR3 = ((ballAngel3 - 90) - yawR3);
            if (angleBallR3 > 0)
            {
                angleBallR3 = 360 - angleBallR3;
            }
            else
            {
                angleBallR3 = Math.Abs(angleBallR3);
            }

            int bxR2 = ln_yawRobot2.StartPoint.X + (int)(Math.Cos(angleBallR2 * (Math.PI / 180)) * ballDist2);
            int byR2 = ln_yawRobot2.StartPoint.Y + (int)(Math.Sin(angleBallR2 * (Math.PI / 180)) * ballDist2);
            ov_ball2.Location = new Point(bxR2 - 12, byR2 - 12);

            int bxR3 = ln_yawRobot3.StartPoint.X + (int)(Math.Cos(angleBallR3 * (Math.PI / 180)) * ballDist3);
            int byR3 = ln_yawRobot3.StartPoint.Y + (int)(Math.Sin(angleBallR3 * (Math.PI / 180)) * ballDist3);
            ov_ball3.Location = new Point(bxR3 - 12, byR3 - 12);

            ln_degRobot2.Visible = false;
            ln_degRobot3.Visible = false;
            #endregion

            #region DataSendtoClientRobot
            //Data TCP Transmit
            dataBroadcast = challange + "#" +               //0
                            ref_com + "#" +                 //1
                            hitballR2 + "#" +               //2
                            //postXR2.ToString() + "#" +      //3
                            StringPostXR2 + "#" +
                            //postYR2.ToString() + "#" +      //4
                            StringPostYR2 + "#" +
                            coorYR2.ToString() + "#" +      //5     XR2 in Client
                            coorXR2.ToString() + "#" +      //6     YR2 in Client
                            calibR2.ToString() + "#" +      //7
                            inputR2.ToString() + "#" +      //8
                            hitballR3 + "#" +               //9
                            //postXR3.ToString() + "#" +      //10
                            StringPostXR3 + "#" +
                            //postYR3.ToString() + "#" +      //11
                            StringPostYR3 + "#" +
                            coorYR3.ToString() + "#" +      //12    XR3 in Client
                            coorXR3.ToString() + "#" +      //13    YR3 in Client
                            calibR3.ToString() + "#" +      //14
                            inputR3.ToString() + "#" +      //15
                            mode.ToString() + "#" +         //16
                            keeperSend + "#" +              //17
                            offSetyawK + "#" +              //18
                            offSetyawR2 + "#" +             //19
                            offSetyawR3 + "#" +             //20
                            kepeerMovement.ToString() + "#" +   //21
                            coorXR1.ToString() + "#" +          //22
                            coorYR1.ToString() + "#" +          //23
                            calibR1.ToString() + "#" +          //24
                            forceCalib.ToString() + "#" +       //25
                            colorTieam.ToString() + "#" +         //26
                            keeper_com;                         //27
            
            if (dataBroadcast != "")
            {
                try
                {
                    if (client1IP != null) BaseServer.SendTo("Robot1", dataBroadcast);
                    if (client2IP != null) BaseServer.SendTo("Robot2", dataBroadcast);
                    if (client3IP != null) BaseServer.SendTo("Robot3", dataBroadcast);
                }
                catch (Exception ex) { }
            }
            tb_datasend.Text = dataBroadcast;
            #endregion
        }

        private void rb_mode1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rb_field1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rectangleShape1_Click(object sender, EventArgs e)
        {

        }
    }
}