using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common_Library;
namespace ChatLa.Server.Models
{
    public class Room : RoomBase
    {
        private const int MAXUSER = 5;
        public List<User> Users { set; get; }
        public bool Started { get; set; }
        public Room() : base()
        {
            Id = Guid.NewGuid().ToString();
            Users = new List<User>();
        }
        public bool IsFull()
        {
            if (Type == Common_Library.Type.Public)
                return Users.Count < MAXUSER ? false : true;
            return Users.Count < 2 ? false : true;
        }
        public bool IsFull(Common_Library.Type type)
        {
            if (type != Type)
                return true;
            return IsFull();
        }
        public void AddNewUser(string ConnectionId, string chatname, Models.Account account, bool isadmin = false)
        {
            Users.Add(new User() { ConnectionId = ConnectionId, ChatName = chatname, IsAdmin = isadmin, Account = account != null ? new Common_Library.AccountBase() { UserName = account.UserName } : null });
        }
        public void AddNewUser(User user)
        {
            user.IsAdmin = false;
            user.AllowInviteFriend = true;
            if (Users.Count == 0)
                Users.Add(user);
            else
            {
                if (Users[0].IsAdmin)
                    Users.Add(user);
                else
                {
                    user.IsAdmin = true;
                    Users.Insert(0, user);
                }
            }
        }
        public static Room CreateNewPrivateRoom(User user1, User user2)
        {
            Room room = new Room() { Type = Common_Library.Type.Private, Started = true };
            room.AddNewUser(user1);
            room.AddNewUser(user2);
            return room;
        }
        public static Room CreateNewRoom(string ConnectionId, string chatname, Common_Library.Type type, Models.Account account)
        {
            Room room = new Room() { Type = type };
            room.AddNewUser(ConnectionId, chatname, account, true);
            return room;
        }
        public bool ContainUserHasChatName(string ChatName)
        {
            return Users.FirstOrDefault(u => u.ChatName == ChatName) != null ? true : false;
        }
        public bool ContainUserHasUserName(string UserName)
        {
            return Users.FirstOrDefault(u => u.Account.UserName == UserName && u.Account != null) != null ? true : false;
        }
        public bool ContainUser(string ConnectionId)
        {
            return Users.FirstOrDefault(u => u.ConnectionId == ConnectionId) != null ? true : false;
        }
        public User GetUserById(string ConnectionId)
        {
            return Users.FirstOrDefault(u => u.ConnectionId == ConnectionId);
        }
        public User GetUserByUserName(string username)
        {
            return Users.FirstOrDefault(u => u.HasAccount && u.Account.UserName == username);
        }
        public void RemoveUser(User user, bool resetadmin = true)
        {
            Users.Remove(user);
            if (resetadmin && Users.Count > 0)
            {
                Users[0].IsAdmin = true;
                Users[0].AllowInviteFriend = true;
            }
        }
    }
}
