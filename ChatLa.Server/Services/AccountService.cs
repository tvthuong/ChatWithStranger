using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatLa.Server.Services
{
    public class AccountService
    {
        public AccountService() { }
        public bool Add(Models.Account account)
        {
            if (DataBaseService.DataBase.Accounts.FirstOrDefault(a => a.UserName == account.UserName) != null)
                return false;
            DataBaseService.DataBase.Accounts.InsertOnSubmit(Encoding(account));
            DataBaseService.DataBase.SubmitChanges();
            return true;
        }
        private Models.Account Encoding(Models.Account account)
        {
            byte[] hashpassword = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(account.Password));
            string password = "";
            foreach (byte e in hashpassword)
                password += e.ToString("X2");
            account.Password = password;
            return account;
        }
        public void ChangePassword(Models.Account account, string NewPassword)
        {
            account = Encoding(account);
            Models.Account current =  DataBaseService.DataBase.Accounts.FirstOrDefault(a => a.UserName == account.UserName && a.Password == account.Password);
            if(current != null)
            {
                account.Password = NewPassword;
                account = Encoding(account);
                current.Password = account.Password;
                DataBaseService.DataBase.SubmitChanges();
            }
        }

        public bool LogIn(Models.Account account)
        {
            account = Encoding(account);
            if (DataBaseService.DataBase.Accounts.FirstOrDefault(a => a.UserName == account.UserName && a.Password == account.Password) != null)
                return true;
            return false;
        }
        public Models.Account GetAccount(Models.Account account)
        {
            account = Encoding(account);
            return DataBaseService.DataBase.Accounts.FirstOrDefault(a => a.UserName == account.UserName && a.Password == account.Password);
        }
        public Models.Account GetAccountHasUserName(string username)
        {
            return DataBaseService.DataBase.Accounts.ToList().FirstOrDefault(a => a.UserName == username);
        }
    }
}
