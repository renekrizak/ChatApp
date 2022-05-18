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
using System.Configuration;

/*
 ---------------------------------
        Server DB structure
 ---------------------------------
 */

/* CREATE TABLE "User" (
	"UserID"	INTEGER NOT NULL UNIQUE,
	"Username"	TEXT NOT NULL UNIQUE,
	"Password"	TEXT NOT NULL,
	"Email"	TEXT NOT NULL,
	PRIMARY KEY("UserID" AUTOINCREMENT)
);
 */

/*CREATE TABLE "Room" (
	"RoomID"	TEXT NOT NULL UNIQUE,
	"RoomPWD"	INTEGER NOT NULL,
	PRIMARY KEY("RoomID")
);
*/

/*CREATE TABLE "UserRooms" (
	"UserID"	INTEGER NOT NULL,
	"RoomID"	TEXT NOT NULL,
	FOREIGN KEY("UserID") REFERENCES "User"("UserID")
);
 */

/*CREATE TABLE "RoomLogs" (
	"UserID"	INTEGER NOT NULL,
	"Time"	TEXT NOT NULL,
	"Message"	TEXT NOT NULL,
	"RoomID"	TEXT NOT NULL,
	FOREIGN KEY("RoomID") REFERENCES "Room"("RoomID"),
	FOREIGN KEY("UserID") REFERENCES "User"("UserID")
);
 */


namespace Server
{
     class Server
    {
		private static string LoadConnectionString(string id = "Default")
		{
			return ConfigurationManager.ConnectionStrings[id].ConnectionString;
		}
		static void Main(string[] args)
        {
        }
    }

    class SocketListener
    {
        public static void StartListening()
        {

        }
    }
}
