using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class WorldFactory
    {
        private const string SERVERS_FILE = @"worlds_data.dat";

        private static readonly int _size = 0x50;//0x50; //account data chunk size      
        private static readonly string _userFilePath = Preferences.Instance.Options.UserFilesPath;
        private static readonly Log _log = Log.Instance;           

        public static void SendWorldList(Socket handler, Blowfish blowfish)
        {
            GamePacket worldList = new GamePacket
            {
                Opcode = 0x15,
                Data = GetWorldListData()
            };

            Packet worldListPacket = new Packet(worldList);
            handler.Send(worldListPacket.ToBytes(blowfish));
            _log.Info("World list sent.");
        }

        private WorldFactory() => LoadWorlds();        

        public static void Delete()
        {
            throw new NotImplementedException();
        }

        public static void Update()
        {
            throw new NotImplementedException();
        }

        public static World GetWorld(string name) => LoadWorlds().Find(x => x.Name == name);
        public static World GetWorld(byte id) => LoadWorlds().Find(x => x.ServerId == (int)id);
       

        public static void CreateWorldsFile()
        {
            List<World> worlds = new List<World>();

            worlds.Add(new World()
            {
                ServerId = 1,
                Name = "Primal Launcher",
                Population = 0x61,
                Address = "127.0.0.1",
                Port = 54992
            });            
            
            try
            {
                using (var fileStream = new FileStream(_userFilePath + Preferences.AppFolder + SERVERS_FILE, FileMode.Create))
                {
                    var bFormatter = new BinaryFormatter();                   
                    bFormatter.Serialize(fileStream, worlds);  
                }
                _log.Success("World file successfully created.");
            }
            catch (Exception) { _log.Error("Could not create the world file. Please check app permissions."); }
            finally { LoadWorlds(); }
        }

        private static List<World> LoadWorlds()
        {
            List<World> worlds = new List<World>();

            if (File.Exists(_userFilePath + Preferences.AppFolder + SERVERS_FILE))
            {
                try
                {
                    using (var fileStream = new FileStream(_userFilePath + Preferences.AppFolder + SERVERS_FILE, FileMode.Open))
                    {
                        var bFormatter = new BinaryFormatter();                        
                         worlds = (List<World>)bFormatter.Deserialize(fileStream); 
                    }                    
                }
                catch (Exception) {_log.Error("There is a problem with the world file. Please try again."); }
            }
            else           
                CreateWorldsFile();            

            return worlds;
        }

        public static byte[] GetWorldListData()
        {
            List<World> worldList = LoadWorlds();
            int numServers = worldList.Count; //number of worlds stored            
            byte[] serverListData = new byte[(_size * numServers) + 0x10]; //extra 16 bytes to store counter
            serverListData[0x09] = (byte)numServers; //store counter

            for (int i = 0; i < numServers; i++)
            {                
                byte[] name = Encoding.ASCII.GetBytes(worldList[i].Name);
                byte[] server = new byte[_size];
                
                server[0x00] = (byte)worldList[i].ServerId;
                server[0x02] = (byte)i;
                server[0x04] = (byte)worldList[i].Population;  
                Buffer.BlockCopy(name, 0, server, 0x10, name.Length);

                Buffer.BlockCopy(server, 0, serverListData, ((i * _size)+0x10), server.Length);                
            }

            return serverListData;
        }

    }
}
