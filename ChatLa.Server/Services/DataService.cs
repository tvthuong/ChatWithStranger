using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatLa.Server.Services
{
    public static class DataService
    {
        public static AccountService Account { get; set; }
        public static FriendService Friend { get; set; }
        static DataService()
        {
            Account = new AccountService();
            Friend = new FriendService();
        }
    }
}
