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
    public sealed class UserRepository
    {
        private static UserRepository _instance = null;
        private static readonly object _padlock = new object();
        private static List<User> _userList = new List<User>();
        private static readonly Log _log = Log.Instance;
        private static Preferences _preferences = Preferences.Instance;

        private const string USERS_FILE = @"user_data.dat"; 
        
        public User User { get; private set; }

        public static UserRepository Instance
        {
            get {
                lock (_padlock) {
                    if (_instance == null)                    
                        _instance = new UserRepository();                    

                    return _instance;
                }
            }
        }

        private UserRepository() => LoadUsers();

        public void LoadUser(string uname, string pwd)
        {
            User = _userList.Find(x => x.Uname == uname && x.Pwd == pwd);
        }

        #region User CRUD
        public static void AddUser(int id, string name, string pwd)
        {            
            User newUser = new User()
            {
                Id = id,
                Uname = name,
                Pwd = pwd
            };          
            
            newUser.AccountList.Add(new GameAccount()
            {
                Id = 1,
                Name = "FFXIV1.0 Primal Launcher Default Account"
            });   
                        
            _userList.Add(newUser);
        }

        public static void DropUser()
        {
            throw new NotImplementedException();
        }

        public void UpdateUser()
        {
            _userList[_userList.FindIndex(x => x.Id == User.Id)] = User;
            WriteFile("");
        }        
        #endregion

        public static void CreateRepositoryFile()
        {
            AddUser(1,"FFXIVPlayer", "FFXIVPlayer");
            WriteFile("Default user successfully created.");
            LoadUsers();
        }

        private static void WriteFile(string msg)
        {
            try //Create repository file with user list.
            {
                using (var fileStream = new FileStream(_preferences.Options.UserFilesPath + Preferences.AppFolder + USERS_FILE, FileMode.Create))
                {
                    var bFormatter = new BinaryFormatter();
                    try
                    {
                        bFormatter.Serialize(fileStream, _userList);
                    }
                    catch (Exception e) { throw e; }
                }

                if(msg != "")
                    _log.Success(msg);
            }
            catch (Exception e) {_log.Error("Could not write file. Please check app permissions."); throw e; }
        }
             
        private static void LoadUsers()
        {
            if (File.Exists(_preferences.Options.UserFilesPath + Preferences.AppFolder + USERS_FILE))
            {
                try
                {
                    using (var fileStream = new FileStream(_preferences.Options.UserFilesPath + Preferences.AppFolder + USERS_FILE, FileMode.Open))
                    {
                        var bFormatter = new BinaryFormatter();                       
                        try
                        {
                            _userList = (List<User>)bFormatter.Deserialize(fileStream);
                        }
                        catch (Exception e) { throw e; }
                    }
                    _log.Success("Loaded saved users.");
                }
                catch (Exception e) { _log.Error("There is a problem with the users file. Please try again."); throw e; }
            }
            else
            {
                CreateRepositoryFile();
            }         
        }

        public void SendUserCharacterList(Socket handler, Blowfish blowfish)
        {
            GamePacket characterList = new GamePacket
            {
                Opcode = 0x0d,
                Data = User.GetCharacters(0) //only one account supported so far.          
            };

            Packet characterListPacket = new Packet(characterList);
            handler.Send(characterListPacket.ToBytes(blowfish));
            _log.Info("Character list sent.");
        }
    }
}
