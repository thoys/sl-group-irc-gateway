using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using libsecondlife;

namespace LibslGroupToIrcBot
{
    public class ChanCommInfo
    {
        public ChanCommInfo(List<string> channels,LLUUID groupID,string host,int port)
        {
            _channels = channels;
            _groupID = groupID;
            _host = host;
            _port = port;
        }
        public List<string> _channels = new List<string>();
        public LLUUID _groupID;
        public string _host;
        public int _port;
    }
}
