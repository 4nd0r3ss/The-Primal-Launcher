using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Characters
{
    public class WorldRepository
    {
        private static WorldRepository _instance = null;
        private static readonly object _padlock = new object();
        private static List<World> _worldList = new List<World>();
        private static readonly Log _log = Log.Instance;

        private const string WORLD_FILE = @"worlds_data.dat";

        public static WorldRepository Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new WorldRepository();

                    return _instance;
                }

            }
        }


        private WorldRepository() => LoadWorlds();

        #region World CRUD
        public static void Create(int id, string name, int population, string address, ushort port)
        {
            World newWorld = new World()
            {
                Id = id,
                Name = name,
                Population = population,
                Address = address,
                Port = port
            };         

            _worldList.Add(newWorld);
        }

        public static void Delete()
        {
            throw new NotImplementedException();
        }

        public static void Update()
        {
            throw new NotImplementedException();
        }

        public World GetWorld(string name) => _worldList.Find(x => x.Name == name);
        public World GetWorld(byte id) => _worldList.Find(x => x.Id == (int)id);
        #endregion

        public static void CreateRepositoryFile()
        {
            Create(1, "AndreusServer", 0x61, "127.0.0.1", 54992);
            Create(2, "KellyServer", 0x31, "127.0.0.1", 54992);
            
            //Create repository file with user list.
            try
            {
                using (var fileStream = new FileStream(WORLD_FILE, FileMode.Create))
                {
                    var bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(fileStream, _worldList);
                }
                _log.Success("World file successfully created.");
            }
            catch (Exception) { _log.Error("Could not create the world file. Please check app permissions."); }
            finally { LoadWorlds(); }
        }

        private static void LoadWorlds()
        {
            if (File.Exists(WORLD_FILE))
            {
                try
                {
                    using (var fileStream = new FileStream(WORLD_FILE, FileMode.Open))
                    {
                        var bFormatter = new BinaryFormatter();
                        _worldList = (List<World>)bFormatter.Deserialize(fileStream);
                    }
                    _log.Success("Loaded saved users.");
                }
                catch (Exception) { _log.Error("There is a problem with the world file. Please try again."); }
            }
            else
            {
                CreateRepositoryFile();
            }
        }

        public byte[] GetWorldListData()
        {
            int numWorlds = _worldList.Count; //number of worlds stored            
            byte[] worldListData = new byte[(World.SIZE * numWorlds) + 0x10]; //extra 16 bytes to store counter
            worldListData[0x09] = (byte)numWorlds; //store counter

            for (int i = 0; i < numWorlds; i++)
            {                
                byte[] name = Encoding.ASCII.GetBytes(_worldList[i].Name);
                byte[] world = new byte[World.SIZE];
                
                world[0x00] = (byte)_worldList[i].Id;
                world[0x02] = (byte)i;
                world[0x04] = (byte)_worldList[i].Population;  
                Buffer.BlockCopy(name, 0, world, 0x10, name.Length);

                Buffer.BlockCopy(world, 0, worldListData, ((i * World.SIZE)+0x10), world.Length);                
            }

            return worldListData;
        }

    }
}
