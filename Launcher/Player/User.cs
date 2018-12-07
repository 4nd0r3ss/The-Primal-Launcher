using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Characters
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Uname { get; set; }
        public string Pwd { get; set; } = "";        
        public List<GameAccount> AccountList { get; set; } = new List<GameAccount>();
        private byte NumAccounts => (byte)AccountList.Count;
        
        public byte[] GetAccountListData()
        {
            byte[] accountListData = new byte[(GameAccount.SLOT_SIZE * NumAccounts) + 0x10]; //0x10 = slot list header size (fixed)
            //slot list header
            accountListData[0x00] = 0x01; //unknown       
            accountListData[0x09] = (byte)NumAccounts;
            accountListData[0x0a] = 0x04; //unknown
            accountListData[0x0b] = 0x99; //unknown

            //slots data
            for (int i=0;i<NumAccounts;i++)
            {
                if (AccountList[i].Id > 0) //if the account is valid
                {                    
                    byte[] account = new byte[GameAccount.SLOT_SIZE];

                    byte[] name = Encoding.ASCII.GetBytes(AccountList[i].Name);                                     
                    Buffer.BlockCopy(BitConverter.GetBytes((uint)AccountList[i].Id), 0, account, 0x00, 0x04);
                    account[0x04] = (byte)(i+1);
                    Buffer.BlockCopy(name,0,account, 0x08, name.Length);

                    Buffer.BlockCopy(account, 0, accountListData, ((i * GameAccount.SLOT_SIZE)+ 0x10), account.Length);
                }
            }
            File.WriteAllBytes("accounts.txt", accountListData);
            return accountListData;
        }
    }
}
