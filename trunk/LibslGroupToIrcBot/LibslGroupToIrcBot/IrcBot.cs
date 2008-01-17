using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LibslGroupToIrcBot
{
    public class IrcBot
    {
        public delegate void NewMessage(IrcBot bot,string username, string message);
        public event NewMessage OnNewMessage;

        TcpClient ircClient = new TcpClient();
        public ChanCommInfo comminf;
        public bool Online = false;
        public IrcBot(ChanCommInfo _comminf)
        {
            comminf = _comminf;
            Write("Starting IrcBot");
        }
        ~IrcBot()
        {
            Write("Stopping IrcBot");
        }
        public void FireNewMessage(string username, string message)
        {
            if (OnNewMessage != null)
            {
                OnNewMessage(this,username, message);
            }
        }
        public void Write(string msg)
        {
            Console.WriteLine("IrcBot: " + msg);
        }
        public void Connect()//string host,int port)
        {
           // _host = host;
           // _port = port;
            Write("Connecting to " + comminf._host + ":" + comminf._port.ToString());
            ircClient.BeginConnect(comminf._host, comminf._port, new AsyncCallback(OnConnect), null);
        }
        void OnConnect(IAsyncResult result)
        {
            Write("Connected to " + comminf._host + ":" + comminf._port.ToString());
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(DoSendAndReceive));
            Online = true;
            thread.Start();
        }
        bool InChannel = false;
        public void JoinChan(string chan)
        {
            PendingSends.Add("JOIN "+chan+"\r\n");
            //currentchannel = chan;
            InChannel = true;
        }
        public void Message(string line)
        {
            if (InChannel)
            {
                //TODO: FIX THIS
                //PendingSends.Add("PRIVMSG " + currentchannel+" :"+line+"\r\n");
            }
        }
        
        public void RAW(string line)
        {
            if (InChannel)
            {
                PendingSends.Add(line + "\r\n");
            }
        }
        public bool SendReceiveThread = false;
        List<string> PendingSends = new List<string>();
        void DoSendAndReceive()
        {
            SendReceiveThread = true;
            NetworkStream str = ircClient.GetStream();
            while (SendReceiveThread)
            {
                bool oneline = true;
                string line = "";
                while (str.DataAvailable && oneline)
                {
                    byte result = 0;
                    int num = str.ReadByte();
                    result = (byte)num;
                    if (num == '\n')
                    {
                        oneline = false;
                    }
                    if (num == -1)
                    {
                        result = 0;
                    }
                    line += ((char)(result));
                }
                while (PendingSends.Count > 0)
                {
                    Write(PendingSends[0]);
                    byte[] buffer = new System.Text.ASCIIEncoding().GetBytes(PendingSends[0]);
                    str.Write(buffer, 0, buffer.Length);
                    PendingSends.RemoveAt(0);
                }
                if (line != "")
                {
                    HandleInData(line);
                }
            }
            str.Close();
            SendReceiveThread = false;
        }
        public void Quit()
        {
            if (SendReceiveThread)
            {
                PendingSends.Add("QUIT :goodbye!\r\n");
            }
        }
        void HandleInData(string line)
        {
            Write(line);
            if (line.Contains("Checking Ident"))
            {
                PendingSends.Add("NICK GroupGuru\r\n");
                PendingSends.Add("USER  libslgroup 8 *  : LibSL Group\r\n");
                foreach(string chan in comminf._channels)
                {
                    PendingSends.Add("JOIN " + chan + "\r\n");
                }
                InChannel = true;
            }
            if (line.StartsWith("PING :"))
            {
                PendingSends.Add("PO" + line.Substring(2));
            }
            if (line.StartsWith(":") && line.Contains(" PRIVMSG "))//&& line.ToLower().Contains(":!group "))
            {
                int index = line.ToLower().IndexOf(" :")+2;
                string name = line.Substring(line.IndexOf(':') + 1, line.IndexOf('!') - line.IndexOf(':') - 1);
                FireNewMessage(name, line.Substring(index).Replace("\r","").Replace("\n",""));

            }
        }
    }
}
