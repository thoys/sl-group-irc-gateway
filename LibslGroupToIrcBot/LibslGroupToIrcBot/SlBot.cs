using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;

namespace LibslGroupToIrcBot
{
    public class SlBot : SecondLife
    {
        public delegate void NewMessage(LLUUID pGroupID,string username, string message);
        public event NewMessage OnNewMessage;
        //public LLUUID GroupKey = LLUUID.Zero;
        public List<LLUUID> requiredgroups = new List<LLUUID>();
        public Dictionary<LLUUID, Group> GroupList = new Dictionary<LLUUID, Group>();
       // bool joinedsession = false;
        Dictionary<LLUUID, List<string>> MsgsToSend = new Dictionary<LLUUID, List<string>>();
        public SlBot()
        {
            Write("Starting SLBot");
            Network.RegisterCallback(libsecondlife.Packets.PacketType.AvatarAppearance, new NetworkManager.PacketCallback(OnAvatarAppearance));
            Groups.OnCurrentGroups += new GroupManager.CurrentGroupsCallback(Groups_OnCurrentGroups);
            Groups.OnGroupJoined += new GroupManager.GroupJoinedCallback(Groups_OnGroupJoined);
            Groups.OnGroupDropped += new GroupManager.GroupDroppedCallback(Groups_OnGroupDropped);
            Groups.OnGroupCreated += new GroupManager.GroupCreatedCallback(Groups_OnGroupCreated);
            Groups.OnGroupLeft += new GroupManager.GroupLeftCallback(Groups_OnGroupLeft);
            Self.OnGroupChatJoin += new AgentManager.GroupChatJoined(Self_OnGroupChatJoin);
            Self.OnGroupChatLeft += new AgentManager.GroupChatLeft(Self_OnGroupChatLeft);
            Self.OnInstantMessage += new AgentManager.InstantMessageCallback(Self_OnInstantMessage);
            Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
            Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
        }

        void Self_OnGroupChatJoin(LLUUID groupChatSessionID, LLUUID tmpSessionID, bool success)
        {
            if (success)
            {
                if (MsgsToSend.ContainsKey(tmpSessionID))
                {
                    foreach (string str in MsgsToSend[tmpSessionID])
                    {
                        Self.InstantMessageGroup(tmpSessionID, str);
                    }
                    MsgsToSend[tmpSessionID].Clear();
                }
                MsgsToSend.Clear();
                //joinedsession = true;
            }
            else
            {
                Self.RequestLeaveGroupChat(tmpSessionID);
            }
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            Appearance.SetPreviousAppearance(false);
        }
        void OnAvatarAppearance(libsecondlife.Packets.Packet packet, Simulator simulator)
        {

        }
        void Network_OnLogin(LoginStatus login, string message)
        {
            if (login == LoginStatus.Success)
            {
                
            }
        }
        public void MsgGroup(LLUUID groupkey, string msg)
        {
            if (Self.GroupChatSessions.ContainsKey(groupkey))
            {
                if (!MsgsToSend.ContainsKey(groupkey))
                    MsgsToSend[groupkey] = new List<string>();
                MsgsToSend[groupkey].Add(msg);
                Self.RequestJoinGroupChat(groupkey);
            }
            else
            {
                Self.InstantMessageGroup(groupkey, msg);
            }

        }
        public void FireNewMessage(LLUUID pGroupID, string username, string message)
        {
            if (OnNewMessage != null)
            {
                OnNewMessage(pGroupID,username, message);
            }
        }
        void Self_OnInstantMessage(InstantMessage im, Simulator simulator)
        {
            if (GroupList.ContainsKey( im.IMSessionID) && im.FromAgentID != Self.AgentID)
            {
                if (im.Dialog == InstantMessageDialog.SessionSend)
                {
                    FireNewMessage(im.IMSessionID, im.FromAgentName, im.Message);
                }
                if (im.Dialog == InstantMessageDialog.MessageFromAgent)
                {
                    if (im.GroupIM)
                    {
                        FireNewMessage(im.IMSessionID,im.FromAgentName, im.Message);
                    }
                }
            }
        }

        void Self_OnGroupChatLeft(LLUUID groupchatSessionID)
        {
            Self.RequestJoinGroupChat(groupchatSessionID);
        }

        void Groups_OnGroupLeft(LLUUID groupID, bool success)
        {
            if (success)
            {
                Groups.RequestCurrentGroups();
            }
        }

        void Groups_OnGroupCreated(LLUUID groupID, bool success, string message)
        {
            Groups.RequestCurrentGroups();
        }

        void Groups_OnGroupDropped(LLUUID groupID)
        {
            Groups.RequestCurrentGroups();
        }

        void Groups_OnGroupJoined(LLUUID groupID, bool success)
        {
            if (success)
            {
                Groups.RequestCurrentGroups();
            }
        }

        void Groups_OnCurrentGroups(Dictionary<LLUUID, Group> groups)
        {
            GroupList = groups;
            foreach (LLUUID grp in requiredgroups)
            {
                if (!GroupList.ContainsKey(grp))
                {
                    Groups.RequestJoinGroup(grp);
                }
            }
        }
        ~SlBot()
        {
            Write("Stopping SLBot");
        }
        public void Write(string msg)
        {
            Console.WriteLine("SLBot: " + msg);
        }
    }
}
