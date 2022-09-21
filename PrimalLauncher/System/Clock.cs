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
using System.Globalization;

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

        public void UpdateBMG()
        {
            if(User.Instance.Character != null) //is spawned
            {
                if (Time.Equals(_dayTime))
                    World.Instance.SetMusic(User.Instance.Character.GetCurrentZone().MusicSet.DayMusic, MusicMode.FadeStart);

                if (Time.Equals(_nightTime))
                    World.Instance.SetMusic(User.Instance.Character.GetCurrentZone().MusicSet.NightMusic, MusicMode.FadeStart);
            }
           
        } 
    }
}


