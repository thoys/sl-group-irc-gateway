using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using libsecondlife;

namespace LibslGroupToIrcBot
{
    class Program
    {
        static SlBot _slbot = new SlBot();
        static UserInputManager _uim;
        static string Firstname = "ERIC";
        static string Lastname = "LASTNAME";
        static string Password = "*****";

        static List<ChanCommInfo> mCommInfo = new List<ChanCommInfo>();
        static string GroupKey = "32f70505-abae-7d46-5898-3e544bfbcaa4";
        static void Main(string[] args)
        {
            List<string> ircChans = new List<string>();
            ircChans.Add("#libsl-group");
            ircChans.Add("#libsl-test");
            mCommInfo.Add(new ChanCommInfo(ircChans,new LLUUID("32f70505-abae-7d46-5898-3e544bfbcaa4"), "irc.efnet.org",6667));
            ircChans = new List<string>();
            ircChans.Add("#opensim-group");
            mCommInfo.Add(new ChanCommInfo(ircChans, new LLUUID("32f70505-abae-7d46-5898-3e544bfbcaa4"), "kubrick.freenode.net", 6667));
            _uim = new UserInputManager(_slbot);//, _ircbot);
            _uim.Login(Firstname, Lastname, Password, mCommInfo);
            //_uim.SetGroupKey(new libsecondlife.LLUUID(GroupKey));
            _uim.Run();
        }
    }
}
