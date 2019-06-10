namespace Common_Library
{
    public class User
    {
        private bool _allowInviteFriend = true;
        public string ChatName { get; set; }
        public string ConnectionId { get; set; }
        public bool IsAdmin { get; set; }
        public AccountBase Account { get; set; }
        public virtual bool AllowInviteFriend { get => _allowInviteFriend; set => _allowInviteFriend = value; }
        public bool CanInviteFriend => HasAccount && AllowInviteFriend;
        public bool HasAccount => Account != null;
        public User() { }
        protected User(User user)
        {
            ChatName = user.ChatName;
            ConnectionId = user.ConnectionId;
            Account = user.Account;
            IsAdmin = user.IsAdmin;
            AllowInviteFriend = user.AllowInviteFriend;
        }
    }
}
