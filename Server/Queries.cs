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
using System.Threading;
namespace Server
{
     class Queries
    {
		private static string LoadConnectionString(string id = "Default")
		{
			return ConfigurationManager.ConnectionStrings[id].ConnectionString;
		}

		/// <summary>
		/// Query to register new user
		/// </summary>

		public static string registerQuery(string userID, string username, string password, string email)
        {
			//writes all the info into the db and generates user id
			SQLiteConnection conn = new SQLiteConnection(LoadConnectionString());
			SQLiteCommand cmd = conn.CreateCommand();
			conn.Open();
			cmd.Parameters.Add(new SQLiteParameter("@UserID", userID));
			cmd.Parameters.Add(new SQLiteParameter("@Username", username));
			cmd.Parameters.Add(new SQLiteParameter("@Password", password));
			cmd.Parameters.Add(new SQLiteParameter("@Email", email));
			cmd.CommandText = "INSERT INTO User(UserID, Username, Password, Email) VALUES(@userID, @Username, @Password, @Email)";
			cmd.ExecuteNonQuery();
			return userID;

		}

		/// <summary>
		/// Basic auth for user login, need to implement encryption
		/// </summary>
		public static string loginQuery(string username, string password)
        {
			SQLiteConnection conn = new SQLiteConnection(LoadConnectionString());
			SQLiteCommand cmd = conn.CreateCommand();
			conn.Open();
			cmd.CommandText = "SELECT COUNT(*) FROM User";
			int numberOfRows = Convert.ToInt32(cmd.ExecuteScalar());
			SQLiteDataReader read;
			cmd.CommandText = "SELECT Username, Password FROM User";
			read = cmd.ExecuteReader();
			while (numberOfRows > 0)
			{
				read.Read();
				if (username == Convert.ToString(read["Username"]) && password == Convert.ToString(read["Password"]))
				{
					cmd.CommandText = "SELECT UserID FROM User WHERE Username=" + username;
					string userID = Convert.ToString(cmd.ExecuteScalar());
					return userID;
				}
				if (numberOfRows == 1)
				{
					/*Tell user that he entered wrong credentials or the user doesnt exist*/
					return "user not found";
				}
				numberOfRows--;
			}
			return "user not found";
		}

		/// <summary>
		/// Query that checks if username is taken
		/// </summary>
		public static bool checkIfUserExists(string username)
        {
			SQLiteConnection conn = new SQLiteConnection(LoadConnectionString());
			SQLiteCommand cmd = conn.CreateCommand();
			conn.Open();
			SQLiteDataReader read;
			cmd.CommandText = "SELECT Username from USER";
			read = cmd.ExecuteReader();
			read.Read();
			if (Convert.ToString(read["Username"]) == username)
			{
				return false;
			}
			return true;
		}
    }
}
