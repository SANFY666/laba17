using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace laba17
{
    public partial class Form2 : Form
    {
        string userName;
        bool isDisconnecting = false;
        const string host = "127.0.0.1";
        const int port = 8888;
        TcpClient client;
        NetworkStream stream;

        public Form2()
        {
            InitializeComponent();
            btnDisconnect.Enabled = false;
            btnSend.Enabled = false;
        }

        // метод для підключення до сервера
        private void btnConnect_Click(object sender, EventArgs e)
        {
            isDisconnecting = false;
            userName = txtUserName.Text;

            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Введіть ім'я!");
                return;
            }

            client = new TcpClient();
            try
            {
                client.Connect(host, port);
                stream = client.GetStream();

                // відправлення імені на сервер
                byte[] data = Encoding.Unicode.GetBytes(userName);
                stream.Write(data, 0, data.Length);

                // прослуховування
                Task.Run(() => ReceiveMessage());

                txtChat.AppendText($"Ласкаво просимо, {userName}!\r\n");

                btnConnect.Enabled = false;
                txtUserName.ReadOnly = true;
                btnDisconnect.Enabled = true;
                btnSend.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка підключення: " + ex.Message);
            }
        }

        // метод для відправки повідомлення на сервер
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text) && stream != null)
            {
                string message = txtMessage.Text;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // додавання свого повідомлення в свій чат
                txtChat.AppendText($"Ви: {message}\r\n");
                txtMessage.Clear();
            }
        }

        // метод для отримання повідомлень від сервера
        private void ReceiveMessage()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);

                        if (bytes == 0) throw new Exception("Сервер відключився");

                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();

                    this.Invoke(new MethodInvoker(() =>
                    {
                        txtChat.AppendText(DateTime.Now.ToShortTimeString() + " " + message + "\r\n");
                    }));
                }
            }
            catch
            {
                if (!isDisconnecting) // сервер дійсно впав
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        MessageBox.Show("Сервер припинив роботу, вікно чату буде закрито", "Відключення", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Close();
                    }));
                }
            }
        }

        // метод для відключення від сервера
        private void Disconnect()
        {
            stream?.Close();
            client?.Close();

            btnConnect.Enabled = true;
            txtUserName.ReadOnly = false;
            btnDisconnect.Enabled = false;
            btnSend.Enabled = false;
        }

        // метод для обробки кнопки відключення
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            isDisconnecting = true;
            Disconnect();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
    }
}
