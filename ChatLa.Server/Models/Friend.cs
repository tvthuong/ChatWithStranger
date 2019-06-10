using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Text;

namespace ChatLa.Server.Models
{
    [global::System.Data.Linq.Mapping.TableAttribute(Name = "dbo.Friend")]
    public partial class Friend : AddFriendRequest
    {
        private EntityRef<Account> _Account;
        private EntityRef<Account> _Account1;
        public Friend() : base()
        {
            this._Account = default(EntityRef<Account>);
            this._Account1 = default(EntityRef<Account>);
        }
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_FirstMember", DbType = "VarChar(30) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public override string FirstMember
        {
            get
            {
                return base.FirstMember;
            }
            set
            {
                if ((base.FirstMember != value))
                {
                    if (this._Account.HasLoadedOrAssignedValue)
                    {
                        throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
                    }
                    base.FirstMember = value;
                }
            }
        }
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_SecondMember", DbType = "VarChar(30) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public override string SecondMember
        {
            get
            {
                return base.SecondMember;
            }
            set
            {
                if ((base.SecondMember != value))
                {
                    if (this._Account1.HasLoadedOrAssignedValue)
                    {
                        throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
                    }
                    base.SecondMember = value;
                }
            }
        }
        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "Account_Friend", Storage = "_Account", ThisKey = "FirstMember", OtherKey = "UserName", IsForeignKey = true)]
        public Account Account
        {
            get
            {
                return this._Account.Entity;
            }
            set
            {
                Account previousValue = this._Account.Entity;
                if (((previousValue != value)
                            || (this._Account.HasLoadedOrAssignedValue == false)))
                {
                    if ((previousValue != null))
                    {
                        this._Account.Entity = null;
                    }
                    this._Account.Entity = value;
                    if ((value != null))
                    {
                        base.FirstMember = value.UserName;
                    }
                    else
                    {
                        base.FirstMember = default(string);
                    }
                }
            }
        }
        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "Account_Friend1", Storage = "_Account1", ThisKey = "SecondMember", OtherKey = "UserName", IsForeignKey = true)]
        public Account Account1
        {
            get
            {
                return this._Account1.Entity;
            }
            set
            {
                Account previousValue = this._Account1.Entity;
                if (((previousValue != value)
                            || (this._Account1.HasLoadedOrAssignedValue == false)))
                {
                    if ((previousValue != null))
                    {
                        this._Account1.Entity = null;
                    }
                    this._Account1.Entity = value;
                    if ((value != null))
                    {
                        base.SecondMember = value.UserName;
                    }
                    else
                    {
                        base.SecondMember = default(string);
                    }
                }
            }
        }
    }
}
