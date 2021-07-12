using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace PrimalLauncher
{    
    [Serializable]
    public class Zone : Actor
    {
        public string ExcludeActorType { get; set; } = "";
        public uint RegionId { get; set; }
        public string MapName { get; set; }
        public string LocationName { get; set; }
        public bool MountAllowed { get; set; } = true;     
        public string ContentFunction { get; set; }
        public MusicSet MusicSet { get; set; }
        public ZoneType Type { get; set; } = ZoneType.Default;
        public byte PrivLevel { get; set; }
        public List<Actor> Actors { get; set; }    
        public string ActorsToLoad { get; set; }

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

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            if (PrivLevel == 1)
                World.Instance.SendTextEnteredDuty(sender);

            Prepare();
            CreateActor(sender);
            SetSpeeds(sender);
            SetPosition(sender, 1, isZoning);
            SetName(sender);
            SetMainState(sender);            
            SetIsZoning(sender);
            SetLuaScript(sender);
        }       

        public void SpawnActors(Socket sender)
        {       
            try
            {
                for (int i = 0; i < Actors.Count; i++)
                {
                    if(Actors[i].GetType().Name != ExcludeActorType)
                        Actors[i].Spawn(sender);
                }

                ExcludeActorType = "";
            }
            catch (Exception e) { Log.Instance.Error(e.Message); throw e; }
        }

        public virtual void LoadActors()
        {
            //we want to load actors just once.
            if (Actors == null)
            {
                Actors = new List<Actor>();
                List<Actor> actors = ActorRepository.Instance.GetZoneNpcs(Id);                
                int index = 1;

                foreach (Actor actor in actors)
                {
                    actor.Id = 4 << 28 | Id << 19 | (uint)index;
                    Actors.Add(actor);
                    index++;
                }

                Actors.AddRange(ActorRepository.Instance.Aetherytes.FindAll(x => x.Position.ZoneId == Id));
                Log.Instance.Success("Loaded " + Actors.Count + " actors in zone " + LocationName);
            }            
        }

        public void LoadMonsters()
        {

        }

        public uint GetCurrentBGM()
        {
            //if it's past 8 in the morning, play day music.
            if (Clock.Instance.Time == new TimeSpan(7, 0, 0))
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
            return Actors.Find(x => x.ClassId == classId);
        }

        public void SpawnAsPrivate(string function, string excludeOfType = "")
        {
            if(!string.IsNullOrEmpty(function))
                ContentFunction = function;

            PrivLevel = 1;
            Type = ZoneType.Nothing;

            ExcludeActorType = excludeOfType;
        }

        public Position GetEntrypoint()
        {
            return new Position();
        }
    }
}


