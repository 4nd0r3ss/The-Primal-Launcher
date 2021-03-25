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
        EventResult = 0x12e,
        PlayerPosition = 0xca,
        Unknown0x02 = 0x02,
        Unknown0x07 = 0x07,
        ChatMessage = 0x03,
        SelectTarget = 0xcd,
        LockOnTarget = 0xcc,
        GMTicketActiveRequest = 0x1d3,
        FriendListRequest = 0x1ce,
        BlacklistRequest = 0x1cb,
        Initialize = 0x06,
        InitGroupWork = 0x133,
        CutSceneFinished = 0x0ce
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
        SetQuestIcon = 0xe3,
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
        SetEventStatus = 0x136,
        ActorInit = 0x137,
        CommandResultX1 = 0x139,
        CommandResult = 0x13c,
        DoEmote = 0xe1,
        ChangeJob = 0x1a4,
        PlayAnimationEffect = 0xda,
        SetUIControl = 0x193,
        SendBlackList = 0x1cb,
        SendFriendList = 0x1ce,        
        StartEvent = 0x12f,
        ChatMessage = 0x03,

        //Delete actors
        MassDeleteStart = 0x06,
        MassDeleteEnd = 0x07,

        //Targeting
        UnloadClassScript = 0xcd,
        SetTarget = 0xd3,
        LoadClassScript = 0xcc,


        //text sheet 
        TextSheetMessage30b = 0x157,
        TextSheetMessage50b = 0x15a,
        TextSheetMessage70b = 0x15b,
        TextSheetMessageNoSource28b = 0x166,

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
        SetHasGobbue = 0x1a1,

        BattleActionResult01 = 0x139,
        EndClientOrderEvent = 0x131,

        //event conditions
        TalkEvent = 0x012e,
        NoticeEvent = 0x016b,
        EmoteEvent = 0x016c,
        PushEventCircle = 0x016f,
        PushEvenFan = 0x0170,
        PushEventTriggerBox = 0x0175,

        StartEventRequest = 0x130,
        //EventResult = 0x12e,

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

        //Gear
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

        GMTicketActiveRequest = 0x1d3,

        //Groups
        GroupHeader = 0x017c,
        GroupBegin = 0x017d,
        GroupEnd = 0x017e,
        GroupMembers = 0x017f,
        GroupOccupancy = 0x0187,
        GroupSync = 0x1020, //check this        
        GroupInitWork = 0x17a,

        ActiveLinkshell = 0x18a
    }

    public enum Command
    {
        ChangeEquipment = 0x2ee9,
        MountChocobo = 0x2eee,
        UmountChocobo = 0x2eef,
        EquipSoulStone = 0x2ef1,

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
        KeyItems = 0x1f4,
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

    public enum AnimationEffect
    {
        ChangeClass = 0x02,
        ChangeTo_WAR = 0x27,
        ChangeTo_MNK = 0x28,
        ChangeTo_PAL = 0x29,
        ChangeTo_WHM = 0x30,
        ChangeTo_BLM = 0x31,
        ChangeTo_DRG = 0x32,
        ChangeTo_BRD = 0x33,

        Teleport = 0xffb,
    }

    [Serializable]
    public enum Weather
    {
        Clear = 8001,
        Fine = 8002,
        Cloudy = 8003,
        Foggy = 8004,
        Windy = 8005,
        Blustery = 8006,
        Rainy = 8007,
        Showery = 8008,
        Thundery = 8009,
        Stormy = 8010,
        Dusty = 8011,
        Sandy = 8012,
        Hot = 8013,
        Blistering = 8014,
        Snowy = 8015,
        Wintry = 8016,
        Gloomy = 8017,
        Seasonal = 8027,
        Primal = 8028,
        Fireworks = 8029,
        Dalamud = 8030,
        Aurora = 8031,
        Dalamudthunder = 8032,
        Day = 8065,
        Twilight = 8066
    }

    [Serializable]
    public enum CityState
    {
        Limsa = 193,
        Gridania = 166,
        UlDah = 184
    }

    public enum AetheryteType
    {
        Crystal = 0x51a6,
        Shard = 0x51a7,
        Gate = 0x51bd
    }

    public enum Region
    {
        //Thanalan = 
    }

    public enum EventType
    {
        //0 (CommandContent), 1 (TalkEvent), 2 (PushDefault), 3 (EmoteDefault1), 5 (NoticeEvent).
        CommandContent = 0,
        TalkEvent = 1,
        PushDefault = 2,
        EmoteDefault1 = 3,
        NoticeEvent = 5
    }

    public enum MessageType
    {
        None = 0,
        Say = 1,
        Shout = 2,
        Tell = 3,
        Party = 4,

        LinkShell1 = 5,
        LinkShell2 = 6,
        LinkShell3 = 7,
        LinkShell4 = 8,
        LinkShell5 = 9,
        LinkShell6 = 10,
        LinkShell7 = 11,
        LinkShell8 = 12,

        SaySpam = 22,
        ShoutSpam = 23,
        TellSpam = 24,
        CustomEmote = 25,
        EmoteSpam = 26,
        StandardEmote = 27,
        UrgentMessage = 28,
        GeneralInfo = 29,
        System = 32,
        SystemError = 33
    }

    public enum UIControl
    {
        Off = 0x14,
        On  = 0x15
    }

    /// <summary>
    /// Hex IDs of the differente types of groups.
    /// </summary>
    [Serializable]
    public enum GroupType
    {
        None = 0,
        Retainer = 0x013881,
        Party = 0x002711,
        Linkshell = 0x004e22,
    }

    public enum DirectorCode
    {
        Opening = 0x00000866
    }

    [Serializable]
    public enum MainStateType
    {
        Default = 0,
        Player = 0xBF,
        Monster = 0x03
    }
    
    [Serializable]
    public enum MainState
    {
        //From http://ffxivclassic.fragmenterworks.com/wiki/index.php/Game_Opcodes:Set_Actor_MainState
        Passive = 0x00,	//Passive; default state of all actors, shows field idle.
        Dead = 0x01,	//Dead; changes the nameplate and disables targeting and commands.
        Active = 0x02,	//Active; starts battle idle and allows for battle commands to fire.
        Dead2 = 0x03,    //Dead 2; unknown.
        SitObject = 0x0A,    //Sitting (Object); used when sitting on a bed or bench.
        SitFloor = 0x0D,    //Sitting (Floor); used when sitting on the floor.
        Unknown = 0x0E,	//??
        Mounting = 0x0F,	//Mounted; plays a mount/dismount animation on transition.
        CraftStance = 0x1E,	//Crafting; kneels down to prepare
        CraftMainTool = 0x1F,	//Crafting; brings out main hand tool
        CraftOffTool = 0x20,	//Crafting; brings out offhand tool
        GatheringMainTool = 0x32,	//Gathering; brings out mainhand tool
        GatheringOffTool = 0x33,	//Gathering; also brings out mainhand tool?
        Debug1 = 0x51,	//Debug? - Wields weapon, locks in place
        Debug2 = 0x5B,	//Debug? - Uses Active pose without grabbing weapon, locks in place
        Debug3 = 0x5C,	//Debug? - Summons chocobo, but no chocobo
    }

    [Serializable]
    public enum ActorSpeed
    {
        Stopped = 0,
        Walking = 0x40000000,
        Running = 0x40d00000,
        Active = 0x40a00000
    }
}
