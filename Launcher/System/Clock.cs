using System;
using System.Globalization;
using System.Net.Sockets;

namespace PrimalLauncher
{
    class Clock
    {
        private static Clock _instance = null;
        private static readonly object _padlock = new object();
        private DateTime _dateTime;
        private readonly double _eorzeaMultiplier = 3600D / 175D;
        private readonly DateTime _epoch = new DateTime(1970, 1, 1);
        private readonly TimeSpan _dayTime = new TimeSpan(7, 0, 0); //7AM
        private readonly TimeSpan _nightTime = new TimeSpan(19, 0, 0); //7PM

        public static Clock Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new Clock();
                    return _instance;
                }
            }
        }
        public string StringTime
        {
            get
            {
                Update();
                return _dateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
            }
        }
        public string Period
        {
            get
            {
                Update();
                return _dateTime.ToString("tt", CultureInfo.InvariantCulture);
            }
        }

        public TimeSpan Time
        {
            get
            {
                Update();
                return _dateTime.TimeOfDay;
            }
        }

        private Clock() { }
        public void Update()
        {
            long timeInSeconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long eorzeaDateTime = (long)(timeInSeconds * (_eorzeaMultiplier));
            _dateTime = _epoch + TimeSpan.FromMilliseconds(eorzeaDateTime);
        }

        public void UpdateBMG(Socket sender)
        {
            if(User.Instance.Character != null) //is spawned
            {
                if (Time.Equals(_dayTime))
                    World.Instance.SetMusic(sender, User.Instance.Character.GetCurrentZone().MusicSet.DayMusic, MusicMode.FadeStart);

                if (Time.Equals(_nightTime))
                    World.Instance.SetMusic(sender, User.Instance.Character.GetCurrentZone().MusicSet.NightMusic, MusicMode.FadeStart);
            }
           
        } 
    }
}


