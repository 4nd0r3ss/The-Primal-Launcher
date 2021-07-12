using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    [Serializable]
    public class CharaWork
    {
        public readonly uint DepictionJudge = 0xA0F50911;
        public readonly byte CommandBorder = 0x20;

        public uint CurrentContentGroup { get; set; }

        public byte[] Property { get; set; } = new byte[0x20];       
        public uint[] StatusShownTime { get; set; }

        public ushort[] Command { get; set; } = new ushort[0x20]; //original size is 0x40. here only the size of commandborder, the rest is hotbar.
        public byte[] CommandCategory { get; set; } = new byte[0x40];
        public bool[] CommandAquired { get; set; } = new bool[0x1000];
        public bool[] AdditionalCommandAquired { get; set; } = new bool[0x24];    

        public CharaWork()
        {
            //descriptions from game data file. 
            Command[0x00] = 21001; //active mode
            Command[0x01] = 21001; //active mode
            Command[0x02] = 21002; //passive mode
            Command[0x03] = 12004; //Begin the designated Battle Regimen.
            Command[0x04] = 21005; //Cast magic quickly at reduced potency.
            Command[0x05] = 21006; //Stop casting a spell.
            Command[0x06] = 21007; //use item
            Command[0x07] = 12009; //equip items
            Command[0x08] = 12010; //set abilities
            Command[0x09] = 12005; //attribute points
            Command[0x0A] = 12007; //skill change
            Command[0x0B] = 12011; //Place marks on enemies to coordinate your party's actions.
            Command[0x0C] = 22012; //Bazaar
            Command[0x0D] = 22013; //Repair
            Command[0x0E] = 29497; //Engage in competitive discourse to win what you seek.
            Command[0x0F] = 22015; //[no description] 


        }
    }

    [Serializable]
    public class ParameterSave
    {
        //public 
    }

    [Serializable]
    public class ParameterTemp
    {

    }

    [Serializable]
    public class BattleSave
    {

    }

    [Serializable]
    public class BattleTemp
    {

    }

    [Serializable]
    public class EventSave
    {

    }

    [Serializable]
    public class EventTemp
    {

    }
}
