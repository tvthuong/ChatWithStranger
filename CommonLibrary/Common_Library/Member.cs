using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Library
{
    public class Member : User
    {
        public Action<string, bool> Action { get; set; }
        public override bool AllowInviteFriend
        {
            get => base.AllowInviteFriend;
            set
            {
                base.AllowInviteFriend = value;
                Action?.Invoke(ConnectionId, AllowInviteFriend);
            }
        }
        public bool IsFriend { get; set; }
        public bool IsNotFriend => !IsFriend;
        public bool CanRequestPrivateChat { get; set; }
        public bool CanRemoveOrAllowInviteFriend { get; set; }
        public bool IsVisibleAllowInviteFriend => CanRemoveOrAllowInviteFriend && HasAccount;
        public bool CanAddFriend => Account != null && IsNotFriend;
        public Member() : base() { }
        public Member(User user) : base(user) { }
    }
}
