using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatLa.Server.Services
{
    public static class DataBaseService
    {
        private static DataBaseDataContext s_dataBase;
        private static object Key = new object();
        public static DataBaseDataContext DataBase
        {
            get
            {
                lock (Key)
                {
                    return s_dataBase;
                }
            }
            set
            {
                lock (Key)
                {
                    s_dataBase = value;
                }
            }
        }
        static DataBaseService()
        {
            DataBase = new DataBaseDataContext();
        }
    }
}
