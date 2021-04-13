using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    /// <summary>
    /// This struct is used to keep data about event conditions for actors. Actors have a list of event conditions populated by specialized class' constructor. 
    /// </summary>
    [Serializable]
    public class Event
    {
        public ServerOpcode Opcode { get; set; }
        public string EventName { get; set; }
        public ushort EmoteId { get; set; }
        public float Radius { get; set; }       //circle size
        public byte Priority { get; set; }      //unknown
        public byte Enabled { get; set; }    //0 won't fire event.
        public byte IsSilent { get; set; }      //0x1 do NOT lock UI and player.
        public byte Direction { get; set; }     //possible values: 0x11 leave circle, 0x1 enter circle.
        public uint ServerCodes { get; set; }

        //For BG objects
        public uint BgObjectId { get; set; }
        public uint LayoutId { get; set; }
        public uint ActorId { get; set; } = 0x4;
        public string ReactionName { get; set; }

        public byte Option1 { get; set; }
        public byte Option2 { get; set; }

        public Event() { }
    }
}
