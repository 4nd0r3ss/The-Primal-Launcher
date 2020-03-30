using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public enum ClientOpcode
    {
        Ping = 0x01,
        DataRequest = 0x12f,
        EventRequest = 0x12d,
        PlayerPosition = 0xca,
        Unknown0x02 = 0x02,
        ChatMessage = 0x03,
        SelectTarget = 0xcd,
        LockOnTarget = 0xcc
    }

    public enum ServerOpcode
    {
        Unknown0x02 = 0x02,
        Unknown = 0x0f,
        CreateActor = 0xca,
        SetPosition = 0xce,
        SetSpeed = 0xd0,
        SetAppearance = 0xd6,
        MapUiChange = 0xe2,
        SetName = 0x13d,
        SetMainState = 0x134,
        SetSubState = 0x144,
        SetAllStatus = 0x179,
        SetIcon = 0x145,
        SetIsZoning = 0x17b,
        AchievementPoints = 0x19c,
        AchievementsLatest = 0x19b,
        AchievementsCompeted = 0x19a,
        PlayerCommand = 0x132,
        ActorInit = 0x137,
        CommandResultX1 = 0x139,
        CommandResult = 0x13c,
        DoEmote = 0xe1,

        //Delete actors
        MassDeleteStart = 0x06,
        MassDeleteEnd = 0x07,

        //Targeting
        UnloadClassScript = 0xcd,
        SetTarget = 0xd3,
        LoadClassScript = 0xcc,


        //text sheet 
        TextSheetMessage30b = 0x157,

        //World specific
        SetDalamud = 0x10,
        SetMusic = 0x0c,
        SetWeather = 0x0d,
        SetMap = 0x05,

        //specific to player character
        SetGrandCompany = 0x194,
        SetTitle = 0x19d,
        SetCurrentJob = 0x1a4,
        SetSpecialEventWork = 0x196,
        SetChocoboName = 0x198,
        SetChocoboMounted = 0x197,
        SetHasChocobo = 0x199,

        BattleActionResult01 = 0x139,
        EndClientOrderEvent = 0x131,

        //event conditions
        TalkEvent = 0x012e,
        NoticeEvent = 0x016b,
        EmoteEvent = 0x016c,
        PushEventCircle = 0x016f,
        PushEvenFan = 0x0170,
        PushEventTriggerBox = 0x0175,


        //Inventory
        InventoryStart = 0x016d,
        InventoryEnd = 0x016e,
        ChunkStart = 0x0146,
        ChunkEnd = 0x0147,

        x01InventoryChunk = 0x0148,
        x08InventoryChunk = 0x0149,
        x16InventoryChunk = 0x014a,
        x32InventoryChunk = 0x014b,
        x64InventoryChunk = 0x014c,

        x01SetEquipment = 0x14d,
        x08SetEquipment = 0x14e,
        x16SetEquipment = 0x14f,
        x32SetEquipment = 0x150,
        x64SetEquipment = 0x151,

        x01RemoveEquipment = 0x152,
        x08RemoveEquipment = 0x153,
        x16RemoveEquipment = 0x154,
        x32RemoveEquipment = 0x155,
        x64RemoveEquipment = 0x156,
    }

    public enum MainState
    {
        Passive = 0x00,
        Dead = 0x01,
        Active = 0x02,
        Dead2 = 0x03,
        SitObject = 0x0a,
        SitFloor = 0x0e,
        Mounting = 0x0f
    }

    public enum Animation
    {
        MountChocobo = 0x7c000062
    }

    public enum Command
    {
        ChangeEquipment = 0x2ee9,
        MountChocobo = 0x2eee,
        UmountChocobo = 0x2eef,

        Logout = 0x5e9b,
        Teleport = 0x5e9c,
        DoEmote = 0x5e26,
        BattleStance = 0x5209,
        NormalStance = 0x520a,
        
        
    }

    public enum BGMMode
    {
        Play = 0x01,
        CrossFade = 0x02,
        Layer = 0x03,
        FadeIn = 0x04,
        Channel1 = 0x05,
        Channel2 = 0x06
    }

    [Serializable]
    public enum InventoryMaxSlots
    {
        Bag = 0xc8,
        Currency = 0x140,
        KeyItems = 0x500,
        Loot = 0x0a,
        MeldRequest = 0x04,
        Bazaar = 0x0a,
        Equipment = 0x23,
        GearSlots = 0x14
    }

    [Serializable]
    public enum InventoryType
    {
        Bag = 0,
        Currency = 0x63,
        KeyItems = 0x64,
        Loot = 0x04,
        MeldRequest = 0x05,
        Bazaar = 0x07,
        Equipment = 0xfe
    }
}
