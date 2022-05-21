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
     
	//object for reading client data async
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

		public static void Main(string[] args)
		{
			StartListening();
		}

	}
		
}
