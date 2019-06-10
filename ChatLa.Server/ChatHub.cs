using ChatLa.Server.Services;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChatLa.Server.Models;
namespace ChatLa.Server
{
    public class ChatHub : Hub
    {
        private static object Key = new object();
        private static string CurrentConnectionId = "";
        private static List<AddFriendRequest> addFriendRequests = new List<AddFriendRequest>();
        private static List<OnlineUser> OnlineUsers = new List<OnlineUser>();
        private static List<Room> Rooms = new List<Room>();
        public async override Task OnConnected()
        {
            await base.OnConnected();
            Clients.Client(Context.ConnectionId).Connected();
        }
        public void ChangePassword(Common_Library.AccountBase account, string NewPassword)
        {
            Wait();
            if (DataService.Account.LogIn(new Account() { UserName = account.UserName, Password = account.Password }))
            {
                DataService.Account.ChangePassword(new Account() { UserName = account.UserName, Password = account.Password }, NewPassword);
                Clients.Client(Context.ConnectionId).ChangePasswordComplete();
            }
            else
                Clients.Client(Context.ConnectionId).PasswordIsNotValid();
            Signal();
        }
        public void DeleteFriend(Common_Library.Friend friend)
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            if (onlineUser != null)
                DataService.Friend.Remove(friend.UserName, onlineUser.Account.UserName);
            Clients.Client(Context.ConnectionId).DeleteComplete();
            onlineUser = OnlineUsers.FirstOrDefault(o => o.HasAccount(friend.UserName));
            if(onlineUser != null)
                Clients.Clients(onlineUser.ConnectionIds).DeleteComplete();
            Signal();
        }
        public void PrivateSend(string message, string ConnectionId)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId) && r.ContainUser(ConnectionId));
            Common_Library.User sender = room.GetUserById(Context.ConnectionId);
            Common_Library.User receiver = room.GetUserById(ConnectionId);
            Clients.Client(sender.ConnectionId).NewPrivateMessage(message, sender, receiver);
            Clients.Client(receiver.ConnectionId).NewPrivateMessage(message, sender, receiver);
            Signal();
        }
        public void AddFriendFromSuggestResponse(string username, bool result)
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            addFriendRequests.Remove(addFriendRequests.FirstOrDefault(e => e.HasMembers(onlineUser.Account.UserName, username)));
            if (result)
            {
                DataService.Friend.add(new Friend() { FirstMember = onlineUser.Account.UserName, SecondMember = username });
                Signal();
                GetFriendList();
                GetFriendList(username);
            }
            else
                Signal();
            GetSuggestsAddFriendList();
            GetSuggestsAddFriendList(username);
            GetAddFriendRequest();
        }
        public void AddFriendResponse(string ConnectionId, bool result)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(ConnectionId) && r.ContainUser(Context.ConnectionId));
            Common_Library.User user = room.GetUserById(Context.ConnectionId);
            if (result)
            {
                Common_Library.User user2 = room.GetUserById(ConnectionId);
                DataService.Friend.add(new Friend() { FirstMember = user.Account.UserName, SecondMember = user2.Account.UserName });
            }
            Clients.Client(ConnectionId).AddFriendResponse(user, result);
            Clients.Client(Context.ConnectionId).AddFriendResponse(user, result);
            Signal();
        }
        public void GetAddFriendRequest()
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            ObservableCollection<Common_Library.AccountBase> list = new ObservableCollection<Common_Library.AccountBase>(addFriendRequests.Where(o => o.HasMembersIsReceiver(onlineUser.Account.UserName)).Select(o => new Common_Library.AccountBase() { UserName = o.GetMemberOf(onlineUser.Account.UserName) }));
            Clients.Client(Context.ConnectionId).GetAddFriendRequest(list);
            Signal();
        }
        public void AddFriendRequestFromSuggest(string username)
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            if (addFriendRequests.FirstOrDefault(e => e.HasMembers(onlineUser.Account.UserName, username)) == null)
                addFriendRequests.Add(new AddFriendRequest() { FirstMember = onlineUser.Account.UserName, SecondMember = username });
            Clients.Client(Context.ConnectionId).GetSuggestsAddFriendList(DataService.Friend.GetSuggestsAddFriendOf(onlineUser.Account.UserName, addFriendRequests));
            onlineUser = OnlineUsers.FirstOrDefault(o => o.HasAccount(username));
            if (onlineUser != null)
            {
                Clients.Clients(onlineUser.ConnectionIds).GetSuggestsAddFriendList(DataService.Friend.GetSuggestsAddFriendOf(username, addFriendRequests));
                ObservableCollection<Common_Library.AccountBase> list = new ObservableCollection<Common_Library.AccountBase>(addFriendRequests.Where(o => o.HasMembersIsReceiver(onlineUser.Account.UserName)).Select(o => new Common_Library.AccountBase() { UserName = o.GetMemberOf(onlineUser.Account.UserName) }));
                Clients.Clients(onlineUser.ConnectionIds).GetAddFriendRequest(list);
            }
            Signal();
        }
        public void GetSuggestsAddFriendList(string username)
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasAccount(username));
            if (onlineUser != null)
            {
                ObservableCollection<Common_Library.AccountBase> list = DataService.Friend.GetSuggestsAddFriendOf(onlineUser.Account.UserName, addFriendRequests);
                Clients.Clients(onlineUser.ConnectionIds).GetSuggestsAddFriendList(list);
            }
            Signal();
        }
        public void GetSuggestsAddFriendList()
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            ObservableCollection<Common_Library.AccountBase> list = DataService.Friend.GetSuggestsAddFriendOf(onlineUser.Account.UserName, addFriendRequests);
            Clients.Client(Context.ConnectionId).GetSuggestsAddFriendList(list);
            Signal();
        }
        public void Response(string ConnectionId, bool result)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(ConnectionId) && r.ContainUser(Context.ConnectionId));
            Common_Library.User user = room.GetUserById(Context.ConnectionId);
            if (result)
            {
                Common_Library.User user2 = room.GetUserById(ConnectionId);
                room.RemoveUser(user, false);
                room.RemoveUser(user2);
                if (room.Users.Count > 0)
                {
                    List<string> list = room.Users.Select(u => u.ConnectionId).ToList();
                    Clients.Clients(list).MemberExit(user.ChatName);
                    Clients.Clients(list).MemberExit(user2.ChatName);
                }
                else
                    Rooms.Remove(room);
                room = Room.CreateNewPrivateRoom(user, user2);
                Rooms.Add(room);
            }
            Clients.Client(ConnectionId).Response(user, result);
            Clients.Client(Context.ConnectionId).Response(user, result);
            Signal();
        }
        public void AddFriendrequest(string ConnectionId)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId) && r.ContainUser(ConnectionId));
            Common_Library.User user = room.GetUserById(Context.ConnectionId);
            Clients.Client(ConnectionId).AddFriendrequest(user);
            Signal();
        }
        public void InviteFriendResponse(string ConnectionId, bool result, string chatname)
        {
            Wait();
            Room senderroom = Rooms.FirstOrDefault(r => r.ContainUser(ConnectionId));
            if(senderroom.IsFull())
            {
                Clients.Client(Context.ConnectionId).RoomIsFull();
                return;
            }
            Common_Library.User user;
            Room currentroom = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            if (currentroom != null)
                user = currentroom.GetUserById(Context.ConnectionId);
            else
            {
                OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
                user = new Common_Library.User() { ChatName = chatname, ConnectionId = Context.ConnectionId, Account = new Common_Library.AccountBase() { UserName = onlineUser.Account.UserName } };
            }
            if (result)
            {
                if (currentroom != null)
                {
                    currentroom.RemoveUser(user);
                    if (currentroom.Users.Count > 0)
                        Clients.Clients(currentroom.Users.Select(u => u.ConnectionId).ToList()).MemberExit(user.ChatName);
                    else
                        Rooms.Remove(currentroom);
                }
                senderroom.AddNewUser(user);
                Clients.Clients(senderroom.Users.Where(u => u.ConnectionId != user.ConnectionId).Select(u => u.ConnectionId).ToList()).NewMember(user.ChatName);
            }
            Clients.Client(ConnectionId).InviteFriendResponse(Context.ConnectionId, user.Account.UserName, result, senderroom.Type);
            Clients.Client(Context.ConnectionId).InviteFriendResponse(Context.ConnectionId, user.Account.UserName, result, senderroom.Type);
            Signal();
        }
        public void ChatWithFriendResponse(string ConnectionId, string UserName, bool result, string senderchatname, string receiverchatname)
        {
            Wait();
            Common_Library.User user;
            Room currentroom = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            if (currentroom != null)
                user = currentroom.GetUserById(Context.ConnectionId);
            else
            {
                OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
                user = new Common_Library.User() { ChatName = receiverchatname, ConnectionId = Context.ConnectionId, Account = new Common_Library.AccountBase() { UserName = onlineUser.Account.UserName } };
            }
            if (result)
            {
                if(currentroom != null)
                {
                    currentroom.RemoveUser(user);
                    if (currentroom.Users.Count > 0)
                        Clients.Clients(currentroom.Users.Select(u => u.ConnectionId).ToList()).MemberExit(user.ChatName);
                    else
                        Rooms.Remove(currentroom);
                }
                OnlineUser onlineUser2 = OnlineUsers.FirstOrDefault(o => o.HasAccount(UserName));
                Common_Library.User user2 = new Common_Library.User() { ChatName = senderchatname, ConnectionId = ConnectionId, Account = new Common_Library.AccountBase() { UserName = onlineUser2.Account.UserName } };
                Room room = Room.CreateNewPrivateRoom(user, user2);
                Rooms.Add(room);
            }
            Clients.Client(ConnectionId).ChatWithFriendResponse(Context.ConnectionId, user.Account.UserName, result);
            Clients.Client(Context.ConnectionId).ChatWithFriendResponse(Context.ConnectionId, user.Account.UserName, result);
            Signal();
        }
        public void Chatrequest(string UserName, string senderchatname)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUserHasUserName(UserName));
            if (room != null)
            {
                Common_Library.User user = room.GetUserByUserName(UserName);
                OnlineUser sender = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
                Clients.Client(user.ConnectionId).Chatrequest(sender.Account.UserName, Context.ConnectionId, senderchatname);
            }
            else
            {
                OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasAccount(UserName));
                if (onlineUser == null)
                    Clients.Client(Context.ConnectionId).FriendIsOffline();
                else
                {
                    OnlineUser sender = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
                    Clients.Clients(onlineUser.ConnectionIds).Chatrequest(sender.Account.UserName, Context.ConnectionId, senderchatname);
                }
            }
            Signal();
        }
        public void InviteFriendRequest(string UserName)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUserHasUserName(UserName));
            if (room != null)
            {
                Common_Library.User user = room.GetUserByUserName(UserName);
                OnlineUser sender = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
                Clients.Client(user.ConnectionId).InviteFriendRequest(sender.Account.UserName, Context.ConnectionId);
            }
            else
            {
                OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasAccount(UserName));
                if (onlineUser == null)
                    Clients.Client(Context.ConnectionId).FriendIsOffline();
                else
                {
                    OnlineUser sender = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
                    Clients.Clients(onlineUser.ConnectionIds).InviteFriendRequest(sender.Account.UserName, Context.ConnectionId);
                }
            }
            Signal();
        }
        public void Privatechatrequest(string ConnectionId)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId) && r.ContainUser(ConnectionId));
            Common_Library.User user = room.GetUserById(Context.ConnectionId);
            Clients.Client(ConnectionId).Privatechatrequest(user);
            Signal();
        }
        public Common_Library.Type ChangeRoomType()
        {
            var room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            if (room != null)
            {
                room.Type = Common_Library.Type.Public;
                Clients.Clients(room.Users.Where(u => u.ConnectionId != Context.ConnectionId).Select(u => u.ConnectionId).ToList()).RoomTypeChanged(room.Type);
                return room.Type;
            }
            return Common_Library.Type.Private;
        }
        public void AllowInviteFriendChanged(string connectionid, bool allowinvitefriend)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(connectionid));
            if(room != null)
            {
                var user = room.GetUserById(connectionid);
                user.AllowInviteFriend = allowinvitefriend;
                Clients.Client(connectionid).AllowInviteFriendChanged(user.CanInviteFriend && !room.IsFull());
            }
            Signal();
        }
        public bool AllowInviteFriend()
        {
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            if(room != null)
            {
                var user = room.GetUserById(Context.ConnectionId);
                return user.CanInviteFriend && !room.IsFull();
            }
            return false;
        }
        public bool IsAdmin()
        {
            var room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            var user = room.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            return user.IsAdmin;
        }
        public void SelectRoom(string chatname, string roomid)
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            Room room = Rooms.FirstOrDefault(r => r.Id == roomid);
            if (room != null)
            {
                if (room.IsFull())
                    Clients.Client(Context.ConnectionId).RoomIsFull();
                else
                {
                    if (room.ContainUserHasChatName(chatname))
                        Clients.Client(Context.ConnectionId).ChatNameIsExist();
                    else
                    {
                        room.AddNewUser(Context.ConnectionId, chatname, onlineUser != null ? onlineUser.Account : null);
                        if (room.Users.Count >= 2)
                            if (room.Users.Count == 2)
                                if (!room.Started)
                                {
                                    room.Started = true;
                                    Clients.Clients(room.Users.Select(u => u.ConnectionId).ToList()).Ready(room.Type);
                                }
                                else
                                {
                                    Clients.Client(Context.ConnectionId).Ready(room.Type);
                                    Clients.Clients(room.Users.Where(u => u.ConnectionId != Context.ConnectionId).Select(u => u.ConnectionId).ToList()).NewMember(chatname);
                                }
                            else
                            {
                                Clients.Client(Context.ConnectionId).Ready(room.Type);
                                Clients.Clients(room.Users.Where(u => u.ConnectionId != Context.ConnectionId).Select(u => u.ConnectionId).ToList()).NewMember(chatname);
                            }
                    }
                }
            }
            else
                Clients.Client(Context.ConnectionId).RoomNotExist();
            Signal();
        }
        public void FindRoom(string chatname, Common_Library.Type type)
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            Room room = Rooms.FirstOrDefault(r => !r.IsFull(type));
            if(room != null)
                if(room.ContainUserHasChatName(chatname))
                {
                    Clients.Client(Context.ConnectionId).ChatNameIsExist();
                    Signal();
                    return;
                }
                else
                    room.AddNewUser(Context.ConnectionId, chatname, onlineUser != null ? onlineUser.Account : null);
            else
            {
                room = Room.CreateNewRoom(Context.ConnectionId, chatname, type, onlineUser != null ? onlineUser.Account : null);
                Rooms.Add(room);
            }
            if (room.Users.Count < 2)
                Clients.Client(Context.ConnectionId).Wait();
            else if (room.Users.Count == 2)
                    if (!room.Started)
                    {
                        room.Started = true;
                        Clients.Clients(room.Users.Select(u => u.ConnectionId).ToList()).Ready(room.Type);
                    }
                    else
                    {
                        Clients.Client(Context.ConnectionId).Ready(room.Type);
                        Clients.Clients(room.Users.Where(u => u.ConnectionId != Context.ConnectionId).Select(u => u.ConnectionId).ToList()).NewMember(chatname);
                    }
                else
                {
                    Clients.Client(Context.ConnectionId).Ready(room.Type);
                    Clients.Clients(room.Users.Where(u => u.ConnectionId != Context.ConnectionId).Select(u => u.ConnectionId).ToList()).NewMember(chatname);
                }
            Signal();
        }
        public void Send(string message)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            Common_Library.User user = room.GetUserById(Context.ConnectionId);
            Clients.Clients(room.Users.Select(u => u.ConnectionId).ToList()).NewMessage(user, message);
            Signal();
        }
        public void GetFriendList(string username)
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasAccount(username));
            if (onlineUser != null)
            {
                ObservableCollection<Common_Library.Friend> list = DataService.Friend.GetFriendsOf(onlineUser.Account);
                foreach (var e in list)
                    e.IsOnline = OnlineUsers.FirstOrDefault(o => o.HasAccount(e.UserName)) != null;
                Clients.Clients(onlineUser.ConnectionIds).GetFriendList(list);
            }
            Signal();
        }
        public void GetOnlineFriend()
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(e => e.HasConnectionId(Context.ConnectionId));
            ObservableCollection<Common_Library.AccountBase> list = DataService.Friend.GetOnlineFriendsOf(onlineUser.Account, OnlineUsers, room.Users);
            Clients.Client(Context.ConnectionId).GetOnlineFriend(list);
            Signal();
        }
        public void GetFriendList()
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            if(onlineUser != null)
            {
                ObservableCollection<Common_Library.Friend> list = DataService.Friend.GetFriendsOf(onlineUser.Account);
                foreach (var e in list)
                    e.IsOnline = OnlineUsers.FirstOrDefault(o => o.HasAccount(e.UserName)) != null;
                Clients.Client(Context.ConnectionId).GetFriendList(list);
            }
            Signal();
        }
        public void GetMemberList()
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            var admin = room.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            ObservableCollection<Common_Library.Member> list = new ObservableCollection<Common_Library.Member>(room.Users.Where(u => u.ConnectionId != Context.ConnectionId).Select(u => new Common_Library.Member(u) { CanRequestPrivateChat = !room.IsPrivateRoom, CanRemoveOrAllowInviteFriend = admin.IsAdmin && !room.IsPrivateRoom }));
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            foreach (var e in list)
                if (onlineUser == null)
                    e.IsFriend = true;
                else if (e.Account != null)
                        e.IsFriend = DataService.Friend.IsFriend(onlineUser.Account.UserName, e.Account.UserName);
            Clients.Client(Context.ConnectionId).GetMemberList(list);
            Signal();
        }
        public void GetRoomList()
        {
            Wait();
            ObservableCollection<Common_Library.Room> list = new ObservableCollection<Common_Library.Room>(Rooms.Where(r => !r.IsFull()).Select(r => new Common_Library.Room() { Id = r.Id, Name = string.Format("Room {0}", Rooms.IndexOf(r) + 1), Type = r.Type }));
            Clients.Client(Context.ConnectionId).GetRoomList(list);
            Signal();
        }
        public void CreateAccount(Common_Library.AccountBase a)
        {
            Wait();
            Models.Account account = new Models.Account() { UserName = a.UserName, Password = a.Password };
            if (DataService.Account.Add(account))
                Clients.Client(Context.ConnectionId).CreateAccountSuccessfully();
            else
                Clients.Client(Context.ConnectionId).CreateAccountFailed();
            Signal();
        }
        public void Wait()
        {
            do
            {
                while (!string.IsNullOrEmpty(CurrentConnectionId)) ;
                lock (Key)
                {
                    if (string.IsNullOrEmpty(CurrentConnectionId))
                        CurrentConnectionId = Context.ConnectionId;
                }
            } while (CurrentConnectionId != Context.ConnectionId);
        }
        public void Signal()
        {
            lock (Key)
            {
                CurrentConnectionId = "";
            }
        }
        public void LogInAutomatically(Common_Library.AccountBase a)
        {
            Wait();
            Models.Account account = DataService.Account.GetAccount(new Models.Account() { UserName = a.UserName, Password = a.Password });
            if(account != null)
            {
                OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasAccount(account.UserName));
                if (onlineUser != null)
                    onlineUser.AddConnectionId(Context.ConnectionId);
                else
                {
                    onlineUser = OnlineUser.CreateNewOnlineUser(account, Context.ConnectionId);
                    OnlineUsers.Add(onlineUser);
                }
                Clients.Client(Context.ConnectionId).LogInComplete();
            }
            else
                Clients.Client(Context.ConnectionId).AccountNotExist();
            Signal();
        }
        public void LogIn(Common_Library.AccountBase a)
        {
            Wait();
            Models.Account account = new Models.Account() { UserName = a.UserName, Password = a.Password };
            if(DataService.Account.LogIn(account) && OnlineUsers.FirstOrDefault(o => o.HasAccount(account.UserName)) == null)
                Clients.Client(Context.ConnectionId).LogInSuccessfully();
            else
                Clients.Client(Context.ConnectionId).LogInFailed();
            Signal();
        }
        public void LeaveRoom()
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(Context.ConnectionId));
            if (room != null)
            {
                Common_Library.User user = room.GetUserById(Context.ConnectionId);
                room.RemoveUser(user);
                if (room.Users.Count == 0)
                    Rooms.Remove(room);
                else
                    Clients.Clients(room.Users.Select(u => u.ConnectionId).ToList()).MemberExit(user.ChatName);
            }
            Signal();
        }
        public void LeaveRoom(string ConnectionId)
        {
            Wait();
            Room room = Rooms.FirstOrDefault(r => r.ContainUser(string.IsNullOrEmpty(ConnectionId) ? Context.ConnectionId : ConnectionId));
            if (room != null)
            {
                Common_Library.User user = room.GetUserById(string.IsNullOrEmpty(ConnectionId) ? Context.ConnectionId : ConnectionId);
                room.RemoveUser(user);
                if (room.Users.Count == 0)
                    Rooms.Remove(room);
                else
                {
                    Clients.Clients(room.Users.Select(u => u.ConnectionId).ToList()).MemberExit(user.ChatName);
                    if (!string.IsNullOrEmpty(ConnectionId))
                        Clients.Client(ConnectionId).MustLeaveRoom();
                }
            }
            Signal();
        }
        public void LogOut()
        {
            Wait();
            OnlineUser onlineUser = OnlineUsers.FirstOrDefault(o => o.HasConnectionId(Context.ConnectionId));
            if (onlineUser != null)
                if (onlineUser.CanRemove)
                    OnlineUsers.Remove(onlineUser);
                else
                    onlineUser.RemoveConnectionId(Context.ConnectionId);
            Signal();
        }
        public async override Task OnDisconnected(bool stopCalled)
        {
            LeaveRoom();
            LogOut();
            await base.OnDisconnected(stopCalled);
        }
    }
}
