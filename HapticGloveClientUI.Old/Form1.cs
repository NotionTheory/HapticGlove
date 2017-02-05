using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace HapticGloveClientUI
{
    public partial class Form1 : Form
    {
        private TcpClient server;
        private Stream stream;
        Timer t;

        private CheckBox[] motors;
        private ProgressBar[] sensors;
        public Form1()
        {
            InitializeComponent();
            this.motors = new CheckBox[] {
                this.motor0,
                this.motor1,
                this.motor2,
                this.motor3,
                this.motor4,
            };
            this.sensors = new ProgressBar[] {
                this.sensor0,
                this.sensor1,
                this.sensor2,
                this.sensor3,
                this.sensor4,
                this.sensor5
            };
            this.t = new Timer();
            this.t.Interval = 10;
            this.t.Tick += T_Tick;
            this.t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            if(this.server != null)
            {
                if(this.server.Connected)
                {
                    if(this.server.Available >= 3)
                    {
                        this.stream.Read(temp, 0, 3);
                        if(temp[2] != 255)
                        {
                        }
                        if(0 <= temp[0] && temp[0] < this.sensors.Length)
                        {
                            this.sensors[temp[0]].Value = temp[1];
                        }
                    }
                }
                else
                {
                    this.stream.Dispose();
                    this.stream = null;
                    this.server = null;
                    this.connectButton.Enabled = true;
                }
            }
        }

        private void motor_CheckedChanged(object sender, EventArgs e)
        {
            SendMotorState();
        }

        private byte[] temp = new byte[] { 0,0,0 };

        private void SendMotorState()
        {
            byte state = 0;
            for(int i = 0; i < this.motors.Length; ++i)
            {
                state <<= 1;
                if(this.motors[i].Checked)
                {
                    state |= 1;
                }
            }
            if(this.server?.Connected ?? false)
            {
                temp[0] = state;
                this.stream.Write(temp, 0, 1);
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if(this.server == null)
            {
                this.connectButton.Enabled = false;
                this.server = new TcpClient();
                this.server.Connect("127.0.0.1", 9001);
                this.stream = this.server.GetStream();
            }
        }
    }
}
