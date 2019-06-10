using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ChatLa.Server.Services
{
    public class FriendService
    {
        public FriendService() { }
        public void add(Models.Friend friend)
        {
            DataBaseService.DataBase.Friends.InsertOnSubmit(friend);
            DataBaseService.DataBase.SubmitChanges();
        }
        public void Remove(string first, string second)
        {
            Models.Friend friend = DataBaseService.DataBase.Friends.ToList().FirstOrDefault(e => e.HasMembers(first,second));
            if (friend != null)
            {
                DataBaseService.DataBase.Friends.DeleteOnSubmit(friend);
                DataBaseService.DataBase.SubmitChanges();
            }
        }
        public bool IsFriend(string first , string second)
        {
            return DataBaseService.DataBase.Friends.ToList().FirstOrDefault(e => e.HasMembers(first,second)) != null;
        }
        public ObservableCollection<Common_Library.Friend> GetFriendsOf(Models.Account account)
        {
            return new ObservableCollection<Common_Library.Friend>(DataBaseService.DataBase.Friends.ToList().Where(o => o.HasMembers(account.UserName)).Select(o => new Common_Library.Friend() { UserName = o.GetMemberOf(account.UserName) }));
        }
        public ObservableCollection<Common_Library.AccountBase> GetSuggestsAddFriendOf(string username,List<Models.AddFriendRequest> addFriendRequests)
        {
            var friends = GetFriendsOf(DataService.Account.GetAccountHasUserName(username));
            ObservableCollection<Common_Library.AccountBase> Suggests = new ObservableCollection<Common_Library.AccountBase>();
            foreach(var e in friends)
            {
                var temp = GetFriendsOf(DataService.Account.GetAccountHasUserName(e.UserName));
                foreach (var t in temp)
                {
                    if (!IsFriend(username, t.UserName) && t.UserName != username && addFriendRequests.FirstOrDefault(o => o.HasMembers(t.UserName, username)) == null && Suggests.FirstOrDefault(s => s.UserName == t.UserName) == null)
                        Suggests.Add(new Common_Library.AccountBase() { UserName = t.UserName });
                }
            }
            return Suggests;
        }
        public ObservableCollection<Common_Library.AccountBase> GetOnlineFriendsOf(Models.Account account, List<Models.OnlineUser> onlineUsers, List<Common_Library.User> user)
        {
            return new ObservableCollection<Common_Library.AccountBase>(DataBaseService.DataBase.Friends.ToList().Where(o => o.HasMembers(account.UserName)).Select(o => new Common_Library.Friend() { UserName = o.GetMemberOf(account.UserName) }).Where(o => user.FirstOrDefault(u => u.HasAccount && u.Account.UserName == o.UserName) == null && onlineUsers.FirstOrDefault(onl => onl.HasAccount(o.UserName)) != null));
        }
    }
}
