using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Text;

namespace ChatLa.Server.Models
{
    [global::System.Data.Linq.Mapping.TableAttribute(Name = "dbo.Account")]
    public partial class Account : Common_Library.AccountBase
    {
        public Account() : base() { }
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_UserName", DbType = "VarChar(30) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public override string UserName
        {
            get => base.UserName;
            set => base.UserName = value;
        }
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_Password", DbType = "Char(32)")]
        public override string Password
        {
            get => base.Password;
            set => base.Password = value;
        }
    }
}
