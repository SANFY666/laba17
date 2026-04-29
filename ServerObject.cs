using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace laba17
{
    public class ServerObject
    {
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>();
        public bool IsRunning { get; private set; }

        private Action<string> logCallback;

        public ServerObject(Action<string> logAction)
        {
            logCallback = logAction;
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }

        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null) clients.Remove(client);
        }

        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                IsRunning = true;
                logCallback("Сервер запущено. Очікування підключень...");

                while (IsRunning)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    System.Threading.Thread clientThread = new System.Threading.Thread(new System.Threading.ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                if (IsRunning)
                {
                    logCallback("Помилка: " + ex.Message);
                    Disconnect();
                }
            }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id)
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }

        protected internal void Disconnect()
        {
            if (!IsRunning) return;
            IsRunning = false;
            tcpListener?.Stop();
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            clients.Clear();
        }
        public void Log(string msg) => logCallback(msg);
    }
}
