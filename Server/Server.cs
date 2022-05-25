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

/*
 ---------------------------------
        Server DB structure
 ---------------------------------
 */

/* CREATE TABLE "User" (
	"UserID"	TEXT NOT NULL UNIQUE,
	"Username"	TEXT NOT NULL UNIQUE,
	"Password"	TEXT NOT NULL,
	"Email"	TEXT NOT NULL,
	PRIMARY KEY("UserID")
);
 */

/*CREATE TABLE "Room" (
	"RoomID"	TEXT NOT NULL UNIQUE,
	"RoomPWD"	TEXT NOT NULL,
	PRIMARY KEY("RoomID")
);
*/

/*CREATE TABLE "UserRooms" (
	"UserID"	TEXT NOT NULL,
	"RoomID"	TEXT NOT NULL,
	FOREIGN KEY("UserID") REFERENCES "User"("UserID")
);
 */

/*CREATE TABLE "RoomLogs" (
	"UserID"	TEXT NOT NULL,
	"Time"	TEXT NOT NULL,
	"Message"	TEXT NOT NULL,
	"RoomID"	TEXT NOT NULL,
	FOREIGN KEY("UserID") REFERENCES "User"("UserID"),
	FOREIGN KEY("RoomID") REFERENCES "Room"("RoomID")
);
 */


namespace Server
{
     
	public class StateObject
    {
		//recv buffer size
		public const int BufferSize = 1024;

		//recv buffer
		public byte[] buffer = new byte[BufferSize];

		//recv data string
		public StringBuilder sb = new StringBuilder();

		//client socket
		public Socket workSocket = null;
    }
	public class AsynchronousSocketListener
    {

		//thread signal
		public static ManualResetEvent allDone = new ManualResetEvent(false);

		public AsynchronousSocketListener()
        {

        }

		public static void StartListening()
        {

			//local socket endpoint, dns name etc.
			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

			//socket listener
			Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
				listener.Bind(localEndPoint);
				listener.Listen(100);
				
				while(true)
                {
					//no singal state
					allDone.Reset();

					//makes connection 
					listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);
					allDone.WaitOne();
                }
            } catch (Exception e)
            {
				Console.WriteLine(e.ToString());
            }

        }

		public static void AcceptCallBack(IAsyncResult ar)
		{
			//signal for main thread to continue
			allDone.Set();

			//socket handling client requests
			Socket listener = (Socket)ar.AsyncState;
			Socket handler = listener.EndAccept(ar);

			//creates state object
			StateObject state = new StateObject();
			state.workSocket = handler;
			handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallBack), state);
		}

		public static void ReadCallBack(IAsyncResult ar)
        {
			String content = String.Empty;

			//retrieves state object and handler socekt from async state object
			StateObject state = (StateObject)ar.AsyncState;
			Socket handler = state.workSocket;

			//reads data from client socket
			int bytesRead = handler.EndReceive(ar);

			if(bytesRead > 0)
            {
				//stores recieved data
				state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

				//checks for end of file (eof) tag, if its not there reads more
				content = state.sb.ToString();
				if(content.IndexOf("<EOF>") > -1)
                {
					//displays recieved data
					Console.WriteLine($"Read {content.Length} bytes from socket. \n Data: {content}");
					Send(handler, content);
                } else
                {
					handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallBack), state);
                }
            }
        }

		private static void Send(Socket handler, String data)
        {
			//converts string data to byte data via ascii encoding
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			//starts sending the data to remote device.
			handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), handler);
        }

		private static void SendCallBack(IAsyncResult ar)
        {
			try
            {
				//retrieves socket from state obj
				Socket handler = (Socket)ar.AsyncState;
				//finishes sending data to remote device.
				int bytesSent = handler.EndSend(ar);
				Console.WriteLine($"Sent {bytesSent} bytes to client.");

				handler.Shutdown(SocketShutdown.Both);
				handler.Close();
            } catch(Exception e)
            {
				Console.WriteLine(e.ToString());
            }
        }
		private static string LoadConnectionString(string id = "Default")
		{
			return ConfigurationManager.ConnectionStrings[id].ConnectionString;
		}

		

		//Checks if user alredy exists in the database, if he does returns false and user will be prompted to choose another username
		private bool checkIfUserExists(string username)
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

		//UserID generation
		static private int _InternalCounter;
		public string setID()
		{
			var now = DateTime.Now;
			var days = (int)(now - new DateTime(2000, 1, 1)).TotalDays;
			var seconds = (int)(now - DateTime.Today).TotalSeconds;

			var counter = _InternalCounter++ % 100;

			string nums = days.ToString("00000") + seconds.ToString("00000") + counter.ToString("00");
			string hashedID = GenerateUniqueID(16);
			return hashedID;
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

		public string registerNewUser(string userInput)
		{
			//this part extracts username, password and email while registering new user, ik this is dumb solution
			string username = "";
			string password = "";
			string email = "";
			string userID = setID();
			int lastPos = 0;
			int count = 0;
			for (int i = 0; i < 2; i++)
            {
				while(true)
                {
                    if (userInput[lastPos] != ':' || userInput[lastPos] != ':')
                    {
						lastPos++;
						if(count == 0)
                        {
							username += userInput[lastPos];
                        }
						else if(count == 1)
                        {
							password += userInput[lastPos];
                        }
						else
                        {
							email += userInput[lastPos];
                        }
                    }
					count++;
					break;
                }
            }
			if(!checkIfUserExists(username))
            {
                /*Prompt client to choose another username, low prio task do later*/
            }
            else
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
			return "";
		}

		public string loginExistingUser(string userInput)
        {
			string username = "";
			string password = "";
			int lastPos = 0;
			int count = 0;

			//extracts username and password from client packet
			for(int i = 0; i < 1; i++)
            {
				while(true)
                {
                    if (userInput[lastPos] != ':' || userInput[lastPos] != '<')
                    {
						lastPos++;
						if(count == 0)
                        {
							username += userInput[lastPos];
                        }
						else if(count == 1)
                        {
							password += userInput[lastPos];
                        }
                    }
					count++;
					break;
                }
            }
			SQLiteConnection conn = new SQLiteConnection(LoadConnectionString());
			SQLiteCommand cmd = conn.CreateCommand();
			conn.Open();
			cmd.CommandText = "SELECT COUNT(*) FROM User";
			int numberOfRows = Convert.ToInt32(cmd.ExecuteScalar());
			SQLiteDataReader read;
			cmd.CommandText = "SELECT Username, Password FROM User";
			read = cmd.ExecuteReader();
			while(numberOfRows > 0)
            {
				read.Read();
				if(username == Convert.ToString(read["Username"]) && password == Convert.ToString(read["Password"]))
                {
					cmd.CommandText = "SELECT UserID FROM User WHERE Username=" + username;
					string userID = Convert.ToString(cmd.ExecuteScalar());
					return userID;
                }
				if(numberOfRows == 1)
                {
					/*Tell user that he entered wrong credentials or the user doesnt exist*/
                }
				numberOfRows--;
            }
			return "";
        }
		
		public static void Main(string[] args)
		{
			StartListening();
		}

	}
		
}
