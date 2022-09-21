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
using System.Collections.Generic;
using System.Linq;
namespace PrimalLauncher
{
    [Serializable]
    public class Zone : Actor
    {
        public string NpcFile { get; set; } = "";
        public uint RegionId { get; set; }
        public string MapName { get; set; }
        public string LocationName { get; set; }
        public bool MountAllowed { get; set; } = true;
        public string ContentFunction { get; set; }
        public MusicSet MusicSet { get; set; }
        public ZoneType Type { get; set; } = ZoneType.Default;
        public byte PrivLevel { get; set; }
        public List<Actor> Actors { get; set; }
        public List<Director> Directors { get; set; } = new List<Director>();
        public List<MapObj> MapObjects { get; set; } = new List<MapObj>();
        public string ActorsToLoad { get; set; }

        public int ActorIndex { get; set; }


        public override void Prepare()
        {
            LuaParameters = new LuaParameters
            {
                ActorName = "_areaMaster" + "@0" + LuaParameters.SwapEndian(Id).ToString("X").Substring(0, 4),
                ClassName = ClassName,
                ClassCode = 0x30400000
            };

            LuaParameters.Add("/Area/Zone/" + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(MapName);
            LuaParameters.Add((!string.IsNullOrEmpty(ContentFunction) ? ContentFunction : ""));
            LuaParameters.Add((!string.IsNullOrEmpty(ContentFunction) ? 1 : -1));
            LuaParameters.Add(Convert.ToByte(MountAllowed));

            for (int i = 7; i > -1; i--)
                LuaParameters.Add(((byte)Type & (1 << i)) != 0);
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor();
            SetSpeeds();
            SetPosition(1, isZoning);
            SetName();
            SetMainState();
            SetIsZoning();
            SetLuaScript();

            //if it's a primal map, we set the weather to primal. This will set arena walls automatically.
            if (Id == 238 || Id == 239 || Id == 240)
                World.Instance.SetWeather(Weather.Primal);

            if (PrivLevel == 1)
                World.SendTextSheet(0x853C); //entered instance

            LoadActors();
            SpawnDirectors();
            SpawnMapObjects();
        }

        public virtual void LoadActors()
        {
            if (Actors == null || Actors.Count == 0)
            {       
                Actors = new List<Actor>();
                List<Actor> actors = ActorRepository.GetZoneNpcs(Id, NpcFile);
                actors.AddRange(World.Instance.Aetherytes.FindAll(x => x.Position.ZoneId == Id));
                actors.AddRange(ActorRepository.GetCompanyWarp(Id));
                ActorIndex = 1;
                NpcFile = "";

                foreach (Actor actor in actors)
                {
                    actor.Id = 4 << 28 | Id << 19 | (uint)ActorIndex;

                    if (actor is MapObj obj)
                        MapObjects.Add(obj);
                    else
                        Actors.Add(actor);

                    ActorIndex++;
                }

                Log.Instance.Success("Loaded " + Actors.Count + " actors in zone " + LocationName + " (" + Id + "/0x"+ Id.ToString("X2") +").");
            }
            else
            {
                Reset();
            }
        }

        public void LoadActor(Actor actor)
        {
            actor.Id = 4 << 28 | Id << 19 | (uint)ActorIndex;
            ActorIndex++;
            Actors.Add(actor);
            actor.Spawn();
        }

        public void RemoveActor(uint classId)
        {
            Actor actor = GetActorByClassId(classId);

            if(actor != null)
            {
                actor.Despawn();
                Actors.Remove(actor);
            }           
        }

        public uint GetCurrentBGM()
        {
            //if it's past 8 in the morning, play day music.
            if (Clock.Instance.Time >= new TimeSpan(7, 0, 0) && Clock.Instance.Time <= new TimeSpan(19, 0, 0))
                return MusicSet.DayMusic;
            else
                return MusicSet.NightMusic;
        }

        public List<Actor> GetActorsByClassId(List<uint> classIds)
        {
            List<Actor> result = new List<Actor>();

            if (Actors != null)
                result.AddRange(Actors.Where(x => classIds.Contains(x.ClassId)));

            return result;
        }

        public List<Actor> GetActorsByFamily(string family)
        {
            List<Actor> result = new List<Actor>();

            if (Actors != null)
                result.AddRange(Actors.Where(x => x.Family == family));

            return result;
        }

        public Actor GetActorByClassId(uint classId)
        {
            Actor actor = Actors.Find(x => x.ClassId == classId);

            if(actor == null)
                actor = MapObjects.Find(x => x.ClassId == classId);

            return actor;
        }

        public Actor GetActorById(uint id)
        {
            Zone zone = this;
            return Actors.FirstOrDefault(x => x.Id == id);
        }        

        public Position GetEntrypoint()
        {
            return new Position();
        }

        public void DespawnAllActors()
        {
            foreach (Actor actor in Actors)
            {
                if (actor.Spawned)
                    actor.Despawn();
            }
        }

        public void Reset()
        {
            foreach (Actor actor in Actors)
            {
                actor.Spawned = false;
            }
        }

        public Director GetDirector(string directorName) => Directors.Find(x => x.GetType().Name == directorName + "Director");
        public Director GetQuestDirector(string questName) => Directors.Find(x => ((QuestDirector)x).QuestName == questName);

        public void SpawnDirectors()
        {
            foreach (Director director in Directors)
            {
                director.Spawn();
            }
        }

        public void SpawnMapObjects()
        {
            foreach (MapObj mo in MapObjects)
            {
                mo.Spawn();
            }
        }

        public void ToggleBattleMusic(bool playBattleMusic = false)
        {
            if (playBattleMusic)
            {
                World.Instance.SetMusic(MusicSet.BattleMusic, MusicMode.Play);
            }
            else
            {
                if(Clock.Instance.Time >= new TimeSpan(7, 0, 0) && Clock.Instance.Time < new TimeSpan(19, 0, 0))
                {
                    World.Instance.SetMusic(MusicSet.DayMusic, MusicMode.FadeStart);
                }
                else
                {
                    World.Instance.SetMusic(MusicSet.NightMusic, MusicMode.FadeStart);
                }
            }
        }

        public void ToggleStoppers(bool enabled)
        {
            foreach(Actor actor in Actors)
            {
                if(actor is Object o)
                {
                    List<Event> events = o.Events.FindAll(x => x.Name == "exit" || x.Name == "caution");

                    if (events.Any())
                        foreach (Event e in events)
                            e.Enabled = (byte)(enabled ? 1 : 0);
                }    
            }
        }
    }
}


