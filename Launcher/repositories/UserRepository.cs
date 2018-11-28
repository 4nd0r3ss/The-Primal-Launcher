using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.users
{
    public sealed class UserRepository
    {
        private static UserRepository _instance = null;
        private static readonly object _padlock = new object();
        private static List<User> _userList = new List<User>();
        private static readonly Log _log = Log.Instance;

        private const string USERS_FILE = @"user_data.dat";        

        public static UserRepository Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)                    
                        _instance = new UserRepository();                    

                    return _instance;
                }

            }
        }

        private UserRepository() => LoadUsers();

        #region User CRUD
        public static void AddUser(int id, string name, string pwd)
        {            
            User newUser = new User()
            {
                Id = id,
                Uname = name,
                Pwd = pwd
            };

            //Create default game account
            GameAccount defaultGameAccount = new GameAccount()
            {
                Id = 1,
                Name = "FINAL FANTASY XIV"
            };

            //add default game account to new user
            newUser.AccountList.Add(defaultGameAccount);

            //complete max number of accounts inside the list (needed for packet size[?])
            for (int i = 1; i < User.MAX_ACCOUNTS; i++)
                newUser.AccountList.Add(new GameAccount());

            //Add default user to user list
            _userList.Add(newUser);
        }

        public static void DropUser()
        {
            throw new NotImplementedException();
        }

        public static void UpdateUser()
        {
            throw new NotImplementedException();
        }

        public User GetUser(string uname, string pwd) => _userList.Find(x => x.Uname == uname && x.Pwd == pwd);       
        #endregion

        public static void CreateRepositoryFile()
        {
            AddUser(1,"FFXIVPlayer","");
            //Create repository file with user list.
            try
            {
                using (var fileStream = new FileStream(USERS_FILE, FileMode.Create))
                {
                    var bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(fileStream, _userList);
                }
                _log.Success("Users file successfully created.");
            }
            catch (Exception) { _log.Error("Could not create the users file. Please check app permissions."); }
            finally { LoadUsers(); }
        }

        private static void LoadUsers()
        {
            if (File.Exists(USERS_FILE))
            {
                try
                {
                    using (var fileStream = new FileStream(USERS_FILE, FileMode.Open))
                    {
                        var bFormatter = new BinaryFormatter();
                        _userList = (List<User>)bFormatter.Deserialize(fileStream);
                    }
                    _log.Success("Loaded saved users.");
                }
                catch (Exception) { _log.Error("There is a problem with the users file. Please try again."); }
            }
            else
            {
                CreateRepositoryFile();
            }         
        }
    }
}
