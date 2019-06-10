using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatLa.Server.Models
{
    public class OnlineUser
    {
        public OnlineUser()
        {
            ConnectionIds = new List<string>();
        }
        public Models.Account Account { get; set; }
        public bool CanRemove => ConnectionIds.Count < 2;
        public List<string> ConnectionIds { get; set; }
        public bool HasAccount(string UserName)
        {
            return Account.UserName == UserName;
        }
        public bool HasConnectionId(string ConnectionId)
        {
            return ConnectionIds.FirstOrDefault(e => e == ConnectionId) != null;
        }
        public void AddConnectionId(string ConnectionId)
        {
            ConnectionIds.Add(ConnectionId);
        }
        public void RemoveConnectionId(string ConnectionId)
        {
            ConnectionIds.Remove(ConnectionId);
        }
        public static OnlineUser CreateNewOnlineUser(Account account, string ConnectionId)
        {
            OnlineUser onlineUser = new OnlineUser() { Account = account };
            onlineUser.AddConnectionId(ConnectionId);
            return onlineUser;
        }
    }
}
