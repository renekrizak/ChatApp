using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Dapper;
using System.Data.SQLite;
using System.Security.Cryptography;


namespace Server
{
    class Room
    {
        private string Password;
        public int Port;
        private string ID;
        static private int _InternalCounter = 0;

        public Room(string password)
        {
            Password = password;
            ID = setID();
        }

        //Sets hashed room ID when room is created.
        public string setID()
        {
            var now = DateTime.Now;
            var days = (int)(now - new DateTime(2000, 1, 1)).TotalDays;
            var seconds = (int)(now - DateTime.Today).TotalSeconds;

            var counter = _InternalCounter++ % 100;

            string nums = days.ToString("00000") + seconds.ToString("00000") + counter.ToString("00");
            string hashedID = GenerateUniqueID(8);
            return this.ID = hashedID;
        }
        //ID Generation
        private static readonly RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        public static string GenerateUniqueID(int length)
        {
            int bufferSize = (length * 6 + 7) / 8;

            var buffer = new byte[bufferSize];
            random.GetBytes(buffer);
            return Convert.ToBase64String(buffer).Substring(0, length);
        }
    }
}
