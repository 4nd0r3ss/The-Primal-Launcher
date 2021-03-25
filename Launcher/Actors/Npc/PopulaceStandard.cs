using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class PopulaceStandard : Actor
    {
        public int QuestIcon { get; set; }

        public PopulaceStandard()
        {
            ClassPath = "/chara/npc/populace/";
            ClassName = "PopulaceStandard";
            ClassCode = 0x30400000;

            EventConditions.Add(new EventCondition { Opcode = ServerOpcode.TalkEvent, EventName = "talkDefault", Priority = 0x04 });
            EventConditions.Add(new EventCondition { Opcode = ServerOpcode.NoticeEvent, EventName = "noticeEvent", IsDisabled = 0x01 });
                    
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0, ushort actorIndex = 0)
        {
            Prepare(actorIndex);
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
            LoadActorScript(sender);
            Init(sender);
            SetEventStatus(sender);
            SetQuestIcon(sender);
        }

        private void SetEventStatus(Socket sender)
        {
            byte[] data = new byte[0x48];

            foreach(EventCondition ec in EventConditions)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((uint)0x01), 0, data, 0, sizeof(uint));
                data[0x04] = 0x01;
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(ec.EventName), 0, data, 0x05, ec.EventName.Length);

                SendPacket(sender, ServerOpcode.SetEventStatus, data);
            }            
        }

        private void SetQuestIcon(Socket sender)
        {
            if (QuestIcon >= 0)            
                SendPacket(sender, ServerOpcode.SetQuestIcon, BitConverter.GetBytes((ulong)QuestIcon));            
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

            SendPacket(sender, ServerOpcode.ActorInit, data);
        }
    }
}
