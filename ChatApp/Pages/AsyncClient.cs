using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ChatApp.Pages
{
    public class StateObject
    {
        //client socket
        public Socket workSocket = null;

        //recv buffer size
        public const int BufferSize = 256;

        //recv buffer
        public byte[] buffer = new byte[BufferSize];

        //recv data string
        public StringBuilder sb = new StringBuilder();

    }

    public class AsynchronousClient
    {

        //remote server port num
        private const int port = 11000;

        //ManualResetEvent signal instances
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        //response from server
        private static String response = String.Empty;
        
        public static void  StartUserClient()
        {

        }


        public static string id = "";
        public static string returnUserID()
        {
            return id;
        }

        public static void StartLogRegClient(string flag, string info)
        {
            //conn to server
            try
            {
                //gets host info
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                //creates tcp/ip socket
                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //connects to server
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallBack), client);
                connectDone.WaitOne();


                //flag will be received by user during the login/reg process, he will send his credentials and will receive 
                //his userID & client window will launch 
                if(flag == "REG")
                {
                    Send(client, info);
                    sendDone.WaitOne();
                    
                    while(true)
                    {
                        Receive(client);
                        receiveDone.WaitOne();
                        if(!string.IsNullOrWhiteSpace(response))
                        {
                            id = response;
                            Console.WriteLine(response);
                        }
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                }
                else if(flag == "LOG")
                {
                    Send(client, info);
                    sendDone.WaitOne();

                    while(true)
                    {
                        Receive(client);
                        receiveDone.WaitOne();
                        if(!string.IsNullOrWhiteSpace(response))
                        {
                            id = response;
                            Console.WriteLine(response);
                        }
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                //gets socket from state object
                Socket client = (Socket)ar.AsyncState;

                //completes connection
                client.EndConnect(ar);

                //signal that conn has been made
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                //creates state object
                StateObject state = new StateObject();
                state.workSocket = client;

                //starts receiving data from server
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                //gets state obj and client socket from async state object
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                //reads data from server
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //if client didnt recieve all the data this maks sure he does
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);

                }
                else
                {
                    //if all the data arrives, put it in response
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), client);
        }

        private static void SendCallBack(IAsyncResult ar)
        {
            try
            {
                //gets socket from state obj
                Socket client = (Socket)ar.AsyncState;

                //finishes sending data to server
                int bytesSent = client.EndSend(ar);

                //signal that all data has been sent
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

      /*  public static int main(string[] args)
        {
            StartClient();
            return 0;
        } */


    }
}
