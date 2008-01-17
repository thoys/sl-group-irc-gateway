using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using libsecondlife;

namespace LibslGroupToIrcBot
{
    public class UserInputManager
    {
        SlBot _slbot;
        List<IrcBot> _ircbots = new List<IrcBot>();
        libsecondlife.LoginParams loginparams;
        static List<ChanCommInfo> mCommInfo = new List<ChanCommInfo>();

        public UserInputManager(SlBot slbot)
        {
            Write("Starting UserInputManager");
            Write("Connecting SLBot to UserInputManager");
            _slbot = slbot;

            _slbot.Network.OnLogin += new libsecondlife.NetworkManager.LoginCallback(Network_OnLogin);
            _slbot.OnNewMessage += new SlBot.NewMessage(_slbot_OnNewMessage);
        }
        public IrcBot AddIrcBot(ChanCommInfo comminf)
        {
            IrcBot result = new IrcBot(comminf);
            result.OnNewMessage += new IrcBot.NewMessage(_ircbot_OnNewMessage);
            Write("Connecting IrcBot to UserInputManager");
            return result;
        }

        void _ircbot_OnNewMessage(IrcBot bot,string username, string message)
        {
            string line = "<irc>[" + username + "]: " + message;
            if (_slbot.Network.Connected)
            {
                //foreach (ChanCommInfo i in mCommInfo)
                {

                    _slbot.MsgGroup(bot.comminf._groupID,line);
                }
            }
            Write(line);
        }


        void _slbot_OnNewMessage(LLUUID pGroupID,string username, string message)
        {
            string line = "<group>[" + username + "]: " + message;
            foreach (IrcBot bot in _ircbots)
            {
                if (bot.comminf._groupID==pGroupID && bot.Online)
                {
                    if (message == "!roundhouse")
                    {
                        bot.Message(message);
                    }
                    else
                    {
                        bot.Message(line);
                    }
                }
            }
            _slbot.Write(line);
        }

        void Network_OnLogin(libsecondlife.LoginStatus login, string message)
        {
            if (login == libsecondlife.LoginStatus.Success)
            {
                _slbot.Write("Logged in " + _slbot.Self.Name);
            }
        }
        ~UserInputManager()
        {
            Write("Stopping UserInputManager");
        }
        public void Login(string firstname,string lastname,string password,List<ChanCommInfo> cci)
        {
            mCommInfo = cci;
            loginparams = _slbot.Network.DefaultLoginParams(firstname,lastname,password,"LibSL-GroupChat-To-IRC by Thoys","0.1");
            foreach (ChanCommInfo c in mCommInfo)
            {
                if (!_slbot.requiredgroups.Contains(c._groupID) && c._groupID != LLUUID.Zero)
                {
                    _slbot.requiredgroups.Add(c._groupID);
                }
            }
            _slbot.Network.BeginLogin(loginparams);
            foreach (ChanCommInfo ch in mCommInfo)
            {
                AddIrcBot(ch).Connect();
            }
            //_ircbot.Connect("efnet.xs4all.nl", 6667);
            //_ircbot.currentchannel = ircChan;
        }
      /*  public void SetGroupKey(libsecondlife.LLUUID key)
        {
            _slbot.GroupKey = key;
        }*/
        bool Running = true;
        public void Run()
        {
            while (Running)
            {
                Console.Write("Input> ");
                string input = Console.ReadLine();
                DoCommands(input);
            }
            foreach (IrcBot bot in _ircbots)
            {
                bot.Quit();
            }
            _slbot.Network.Logout();
        }
        void DoCommands(string command)
        {
            string [] args = command.Split(new char[] { ' ' });
            switch (args[0])
            {
                case "quit":
                    Running = false;
                    break;
               /* case "join":
                    if (args.Length > 1)
                    {
                        _ircbot.JoinChan(args[1]);
                    }
                    break;
                case "groupjoin":
                    _slbot.Groups.RequestJoinGroup(_slbot.GroupKey);
                    break;
                case "msg":
                    if (args.Length > 1)
                    {
                        if (_ircbot.Online)
                        {
                            _ircbot.Message(command.Substring(args[0].Length+1));
                        }
                    }
                    break;
                case "raw":
                    if (args.Length > 1)
                    {
                        if (_ircbot.Online)
                        {
                            _ircbot.RAW(command.Substring(args[0].Length + 1));
                        }
                    }
                    break;*/
                case "login":
                    _slbot.Network.BeginLogin(loginparams);
                    break;
                case "logout":
                    _slbot.Network.Logout();
                    break;

            }
        }
        void Write(string msg)
        {
            Console.WriteLine("UserInputManager: " + msg);
        }
    }
}
