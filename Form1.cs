using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace laba17
{
    public partial class Form1 : Form
    {
        ServerObject server; // об'єкт сервера

        public Form1()
        {
            InitializeComponent();
            // логування
            server = new ServerObject(this.Log);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Task.Run(() => server.Listen()); // запуск сервера 
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            server.Disconnect();
            Log("Сервер зупинено.");
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        // метод логування
        public void Log(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), message);
            }
            else
            {
                txtLogs.AppendText(DateTime.Now.ToShortTimeString() + " " + message + "\r\n");
            }
        }

        // метод для відкриття форми клієнта
        private void btnOpenClient_Click(object sender, EventArgs e)
        {
            Form2 clientForm = new Form2();
            clientForm.Show();
        }
    }
}
