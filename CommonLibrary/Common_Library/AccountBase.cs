using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Library
{
    public class AccountBase
    {
        protected string _UserName;
        protected string _Password;
        public virtual string UserName
        {
            get => _UserName;
            set => _UserName = value;
        }
        public virtual string Password
        {
            get => _Password;
            set => _Password = value;
        }
        public AccountBase() { }
    }
}
