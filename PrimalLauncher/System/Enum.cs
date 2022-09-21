/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel.DataAnnotations;

namespace PrimalLauncher
{
    /// <summary>
    /// Opcodes for packets received from game client.
    /// </summary>
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
        CutScene = 0x0ce,
        ItemSearchRequest = 0x1DF,
        RetainerSearchRequest = 0x1D9,
        PurchaseHistoryRequest = 0x1DA
    }

    /// <summary>
    /// Opcodes for packets sent by the server.
    /// </summary>
    public enum ServerOpcode
    {
        Unknown0x02 = 0x02,
        Unknown = 0x0F,
        CreateActor = 0xCA,
        SetPosition = 0xCE,
        MoveToPosition = 0xCF,
        SetSpeed = 0xD0,
        SetAppearance = 0xD6,
        ObjBindToMap = 0xD8,
        ObjPlayAnimation = 0xD9,
        
        MapUiChange = 0xE2,
        SetQuestIcon = 0xE3,
        SetName = 0x13D,
        SetMainState = 0x134,
        SetSubState = 0x144,

        SetStatus = 0x177,
        SetAllStatus = 0x179,
        SetIcon = 0x145,
        SetIsZoning = 0x17B,

        AchievementsCompeted = 0x19A,
        AchievementsLatest = 0x19B,
        AchievementPoints = 0x19C,     
        AchievementUnlocked = 0x19E,

        PlayerCommand = 0x132,
        SetEventStatus = 0x136,
        ActorInit = 0x137,
        CommandResultX1 = 0x139,
        CommandResultX10 = 0x13A,
        CommandResult = 0x13C,
        DoEmote = 0xE1,
        ChangeJob = 0x1A4,
        PlayAnimationEffect = 0xDA,
        SetUIControl = 0x193,
        SendBlackList = 0x1CB,
        SendFriendList = 0x1CE,        
        StartEvent = 0x12F,
        ChatMessage = 0x03,
        GeneralData = 0x133,

        //Delete actors
        MassDeleteStart = 0x06,
        MassDeleteEnd = 0x07,
        MassDeletex1 = 0x08,
        MassDeletex10 = 0x09,
        MassDeletex20 = 0x0A,
        MassDeletex40 = 0x0B,        
        RemoveActor = 0xCB,


        //Targeting
        LoadClassScript = 0xCC,
        UnloadClassScript = 0xCD,
        SetTarget = 0xD3,
        SetHeadToTarget = 0xD3,
        TurnToTarget = 0xD4,
        SetHeadToActor = 0xDB,
        ResetHead = 0xDE,


        //text sheet 
        TextSheet30b = 0x157,
        TextSheet38b = 0x158,
        TextSheet40b = 0x159,
        TextSheet50b = 0x15A,
        TextSheet70b = 0x15B,

        TextSheetCustom48b = 0x15C,
        TextSheetCustom58b = 0x15D,
        TextSheetCustom68b = 0x15E,
        TextSheetCustom78b = 0x15F,
        TextSheetCustom98b = 0x160,

        TextSheetDisplayName48b = 0x15C,
        TextSheetDisplayName58b = 0x15D,
        TextSheetDisplayName68b = 0x15E,
        TextSheetDisplayName78b = 0x15F,
        TextSheetDisplayName70b = 0x160,

        TextSheetNoSource28b = 0x166,
        TextSheetNoSourceWithcode38b = 0x167, //need to figure this out
        TextSheetNoSource38b = 0x168,
        TextSheetNoSource48b = 0x169,
        TextSheetNoSource68b = 0x16A,

        //World specific
        SetDalamud = 0x10,
        SetMusic = 0x0C,
        SetWeather = 0x0D,
        SetMap = 0x05,

        //specific to player character
        SetGrandCompany = 0x194,
        SetEnmity = 0x195,
        SetTitle = 0x19A,
        SetCurrentJob = 0x1A4,
        SetSpecialEventWork = 0x196,
        SetChocoboName = 0x198,
        SetChocoboMounted = 0x197,
        SetHasChocobo = 0x199,
        SetGobbueMounted = 0x01A0,
        SetHasGobbue = 0x1A1,

        //event conditions
        TalkEvent = 0x012e,
        NoticeEvent = 0x016b,
        EmoteEvent = 0x016c,
        PushEventCircle = 0x016f,
        PushEvenFan = 0x0170,
        PushEventTriggerBox = 0x0175,

        EventRequestResponse = 0x130,
        EventRequestFinish = 0x131,

        
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
        GroupDutyMembers = 0x183,

        ActiveLinkshell = 0x18a,
        UnendingJourney = 0x1a3,
        EntrustedItems = 0x1a5,

        Logout = 0x00E,
        ExitGame = 0x011,    

        StartNPCLinkshell = 0x1031,

        ItemSearchStart = 0x01D7,
        ItemSearchresult =0x01D8,
        ItemSearchEnd = 0x01D9,
        ItemSearchClose = 0x01E1,

        RetainerSearchEnd = 0x01DA,
        RetainerSearchResult = 0x01DB,
        RetainerSearchUpdate = 0x01DC,
        
        PurchaseHistory = 0x01DD
    }

    /// <summary>
    /// Commands triggered by different actions in the game.
    /// </summary>
    public enum Command
    {
        ChangeEquipment     = 0x2EE9,
        Mount               = 0x2EEE,
        Umount              = 0x2EEF,
        EquipSoulStone      = 0x2EF1,

        DoEmote             = 0x5E26,
        QuestData           = 0x5E93,
        GuildleveData       = 0x5E94,
        NpcLinkshellChat    = 0x5E95,
        Logout              = 0x5E9B,
        Teleport            = 0x5E9C,        
        PlaceDriven         = 0x5EED,
        
        BattleStance        = 0x5209,
        NormalStance        = 0x520A,
        
        PlayerAutoAttack    = 0x5658,
        MonsterAutoAttack   = 0x59DD
    }
       
    /// <summary>
    /// Used to set the number of slots availble in different inventories.
    /// </summary>
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

    /// <summary>
    /// Animation ids used in throughout the game.
    /// </summary>
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

        TeleportWait = 0xFFA,
        Teleport = 0xFFB,
    }

    /// <summary>
    /// Used to set in-game weather.
    /// </summary>
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

    public enum AetheryteType
    {
        Crystal = 0x51A6,       
        Gate = 0x51BD
    }

    /// <summary>
    /// The different types of actor events.
    /// </summary>
    public enum EventType
    {
        //0 (CommandContent), 1 (TalkEvent), 2 (PushDefault), 3 (EmoteDefault1), 5 (NoticeEvent).
        commandContent = 0,
        talkDefault = 1,
        pushDefault = 2,
        emoteDefault1 = 3,
        noticeEvent = 5
    }

    /// <summary>
    /// Sets the type of message to be sent to the game chat window.
    /// </summary>
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
        UrgentMessage = 28, //light red
        GeneralInfo = 29, //dark purple
        System = 32, //light purple
        SystemError = 33,//light purple
        //38 = white
        NPCLinkshell = 39
            //40 = light purple
    }

    /// <summary>
    /// Used when freezing/unfreezing the UI when in certain events.
    /// </summary>
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
        None        = 0,
        Retainer    = 0x013881,
        Party       = 0x002711,
        Linkshell   = 0x004E22,
        Duty        = 0x007536,
        Monster     = 0x0
    }

    /// <summary>
    /// Used to set the actor type for a given main state.
    /// </summary>
    [Serializable]
    public enum MainStateType
    {
        Default = 0,
        Player = 0xBF,
        Monster = 0x03
    }

    /// <summary>
    /// Known main states for an actor.
    /// </summary>
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

    /// <summary>
    /// Values used to set and actor's movement speed. The higher the value, the faster it moves. Setting a value too high will crash the game.
    /// </summary>
    [Serializable]
    public enum ActorSpeed
    {
        Stopped = 0,
        Walking = 0x40000000,
        Running = 0x40d00000,
        Active = 0x40a00000,

        WalkingMount = 0x40F00000,
        RunningMount = 0x41F00000
    }

    /// <summary>
    /// Set how BGM should be played.
    /// </summary>
    [Serializable]
    public enum MusicMode
    {
        Play = 0x01, //change music immediately
        Crossfade = 0x02,
        Layer = 0x03,
        FadeStart = 0x04,
        ChannelOne = 0x05,
        ChannelTwo = 0x06
    }

    public enum ZoneType
    {
        Nothing = 0,
        Default = 4,
        Inn = 68,
        Instance = 6,
        CanStealth = 132
    }

    public enum LuaIdUint
    {
        //this is used to fake type and int for lua parameters. Will probably be removed later when I come up with something better.
    }

    public enum ChocoboAppearance
    {
        Rental = 0,

        //company chocobo type bound by rank?
        Maelstrom1 = 0x01,
        Maelstrom2 = 0x02,
        Maelstrom3 = 0x03,
        Maelstrom4 = 0x04,
        
        TwinAdder1 = 0x1F,
        TwinAdder2 = 0x20,
        TwinAdder3 = 0x21,
        TwinAdder4 = 0x22,
        
        ImmortalFlames1 = 0x3D,
        ImmortalFlames2 = 0x3E,
        ImmortalFlames3 = 0x3F,
        ImmortalFlames4 = 0x40,
    }

    public enum ShopType
    {
        Item = 0,
        Class = 1,
        Weapon = 2,
        Armor = 3,
        Hamlet = 4
    }

    public enum JobClassCategory
    {
        DoW = 1,
        DoM = 2,
        DoH = 3,
        DoL = 4
    }

    public enum JobClassRole
    {
        Tank = 1,
        Attacker = 2,
        Healer = 3
    }

    public enum LobbyOptions
    {
        [Display(Name = "None")]
        Default,
        [Display(Name = "Inactive character")]
        Inactive,
        [Display(Name = "Rename character")]
        Rename,       
        [Display(Name = "Legacy account")]
        Legacy = 0x08
    }
}
