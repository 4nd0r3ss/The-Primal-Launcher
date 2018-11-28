using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.users
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Uname { get; set; }
        public string Pwd { get; set; } = "";
        public static int MAX_ACCOUNTS = 0x8;
        public List<GameAccount> AccountList { get; set; } = new List<GameAccount>(MAX_ACCOUNTS);
        
        public byte[] GetAccountListData()
        {
            byte[] accountListData = new byte[GameAccount.SIZE * MAX_ACCOUNTS];

            for(int i=0;i<MAX_ACCOUNTS;i++)
            {
                if (AccountList[i].Id > 0) //if the account is valid
                {
                    byte[] name = Encoding.ASCII.GetBytes(AccountList[i].Name);
                    byte[] account = new byte[GameAccount.SIZE];
                    //write account id
                    account[0x00] = (byte)AccountList[i].Id;
                    //write other info (fint out what it is)
                    account[0x08] = 0x01;
                    account[0x09] = 0x01;
                    account[0x10] = 0x01;
                    //write account name
                    Buffer.BlockCopy(name,0,account, 0x18, name.Length);
                    Buffer.BlockCopy(account, 0, accountListData, (i * GameAccount.SIZE), account.Length);
                }
            }          

            return accountListData;
        }
    }
}
