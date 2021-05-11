using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Populace : Actor
    {
       
        public string TalkFunction { get; set; }
        public string QuestFunction { get; set; }

        public Populace()
        {
            ClassPath = "/chara/npc/populace/";           
            ClassCode = 0x30400000;
            TalkFunction = "processEvent000_9";// "processTtrNomal003";
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender, 0x08);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender);
            SetAppearance(sender);
            SetName(sender);
            SetMainState(sender);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIsZoning(sender);
            SetLuaScript(sender);
            Init(sender);
            SetEventStatus(sender);
            SetQuestIcon(sender);
            Spawned = true;
        }               

        public override void Init(Socket sender)
        {
            byte[] data =
            {
                0x5E, 0x04, 0xF7, 0x6D, 0xF6, 0xC0, 0x00, 0x00, 0x80, 0x3F, 0x01, 0xA8, 0x0C, 0x4B, 0xE1, 0x01,
                0x01, 0x71, 0xFD, 0x38, 0x21, 0x01, 0x01, 0xB1, 0xCF, 0xFB, 0xFB, 0x01, 0x02, 0xAA, 0xBC, 0x32,
                0x42, 0xF4, 0x01, 0x02, 0x69, 0xFB, 0xCD, 0x7B, 0xF4, 0x01, 0x02, 0x10, 0x97, 0xF8, 0x13, 0x00,
                0x00, 0x02, 0xC5, 0xD5, 0x95, 0x3C, 0x00, 0x00, 0x02, 0x76, 0xC8, 0x99, 0x0B, 0x00, 0x00, 0x01,
                0x24, 0xCE, 0x32, 0x75, 0x03, 0x01, 0xE3, 0xCF, 0x4F, 0x58, 0x03, 0x02, 0x88, 0x35, 0x06, 0x96,
                0x02, 0x00, 0x01, 0xBE, 0x05, 0x9C, 0x8E, 0x01, 0x88, 0x2F, 0x5F, 0x69, 0x6E, 0x69, 0x74, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            Packet.Send(sender, ServerOpcode.ActorInit, data, Id);
        }

        #region Event methods       
        public override void talkDefault(Socket sender)
        {            
            EventManager.Instance.CurrentEvent.DelegateEvent(sender, 0x1ADB1, TalkFunction);           
        }

        public override void pushDefault(Socket sender)
        {     
            EventManager.Instance.CurrentEvent.DelegateEvent(sender, 0x1ADB1, "processTtrNomal002");            
        }
        #endregion
    }
}
