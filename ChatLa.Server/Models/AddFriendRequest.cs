using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatLa.Server.Models
{
    public class AddFriendRequest
    {
        protected string _FirstMember;
        protected string _SecondMember;
        public virtual string FirstMember
        {
            get => _FirstMember;
            set => _FirstMember = value;
        }
        public virtual string SecondMember
        {
            get => _SecondMember;
            set => _SecondMember = value;
        }
        public AddFriendRequest() { }
        public bool HasMembers(string first, string second)
        {
            return (FirstMember == first && SecondMember == second) || (FirstMember == second && SecondMember == first);
        }
        public bool HasMembers(string username)
        {
            return FirstMember == username || SecondMember == username;
        }
        public bool HasMembersIsReceiver(string username)
        {
            return SecondMember == username;
        }
        public string GetMemberOf(string username)
        {
            return FirstMember == username ? SecondMember : FirstMember;
        }
    }
}
