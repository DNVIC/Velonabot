using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using _602countingbot;
using System.IO;

namespace _602countingbot
{
    
    class Program
    {
        private static string _user;
        private static string _oauth;
        private static string _channel;
        private static string _username;
        private static IPAddress _ip;

        
        public static void assignStrings()
        {
            Console.Write("Insert bot username ");
            string user = Console.ReadLine();

            Console.Write("Insert bot oauth ID ");
            string pass = Console.ReadLine();
            
            Console.Write("Insert channel to autocount in ");
            string chan = Console.ReadLine();
            
            Console.Write("Insert your twitch username ");
            string UserName = Console.ReadLine();
            
            Console.Write("Insert IP from LiveSplit Server ");
            string ip = Console.ReadLine();


            _ip = IPAddress.Parse(ip);
            _user = user;
            _oauth = pass;
            _channel = chan;
            _username = UserName;
        }

        static async Task Main(string[] args)
        {
            assignStrings();
            await ExecuteClient();
        }
        static async Task ExecuteClient()
        {
            // Base socket code taken from https://geeksforgeeks.org/socket-programming-in-c-sharp/
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipaddr = _ip;
                IPEndPoint localEndPoint = new IPEndPoint(ipaddr, 16834);
                Console.WriteLine(ipaddr.ToString());
                Socket sender = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //Connect socket to endpoint
                    sender.Connect(localEndPoint);
                    byte[] ByteBuffer = new byte[1024];



                    //Print information that means we are good
                    Console.WriteLine("Socket connected to -> {0} ", sender.RemoteEndPoint.ToString());

                    /*
                    //Creating sent message
                    byte[] messageSent = Encoding.ASCII.GetBytes("starttimer\r\n");
                    //Sending
                    int byteSent = sender.Send(messageSent);
                    messageSent = Encoding.ASCII.GetBytes("split\r\n");
                    int byte2Sent = sender.Send(messageSent);
                    */

                    await SplitLevelChecker(sender, ByteBuffer);
                    //sender.Shutdown(SocketShutdown.Both);
                    //sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected Exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void SendCommand(Socket s, string Command)
        {
            byte[] spinx = Encoding.ASCII.GetBytes(Command + "\r\n");
            s.Send(spinx);   
        }

        static string ReceiveCommand(Socket s, byte[] Buffer)
        {
            
            int recv = s.Receive(Buffer);

            


            return Encoding.ASCII.GetString(Buffer, 0, recv);
        }
        static string SendAndReceiveCommand(Socket s, string Command, byte[] Buffer)
        {
            byte[] spinx = Encoding.ASCII.GetBytes(Command + "\r\n");
            s.Send(spinx);
            int recv = s.Receive(Buffer);
            return Encoding.ASCII.GetString(Buffer, 0, recv);
        }


        private static async Task SplitLevelChecker(Socket sender, byte[] ByteBuffer )
        {
            Console.Write("Press enter when the 602 race starts (press enter in opening of sm64)"); //Livesplit server acts weird when started and livesplit is not already running
            Console.ReadLine();
            string CurrentProgress = "";
            IrcClient ircClient = new IrcClient("irc.chat.twitch.tv", 6667, _user, _oauth, _channel); // Sets up the connection with the twitch chat

            PingSender ping = new PingSender(ircClient); // Sends a ping every 5 minutes; otherwise twitch will kick the bot
            ping.Start();
            int StarCount = 0; //star count is created outside of the while loops so that it is an external variable

            int PreviousStarCount = 0; //used to calculate when the star count changes
            string[] index = File.ReadAllLines(@"C:\602counting\index.txt");
            while (true)
            {


                string ReceivedCommand = SendAndReceiveCommand(sender, "getsplitindex", ByteBuffer); // Gets the current split from livesplit
                string SplitID = index[int.Parse(ReceivedCommand)]; 
                Console.WriteLine(int.Parse(SplitID));
                Console.WriteLine(int.Parse(ReceivedCommand));
                Console.WriteLine(SplitID);


                StarCount = int.Parse(SplitID);
                Console.WriteLine(StarCount);
                if (StarCount != PreviousStarCount)
                {
                    ircClient.SendPublicChatMessage("!set " + _username + " " + (StarCount).ToString());
                    //ircClient.SendPublicChatMessage("!add " + "DNVIC " + (StarCount - PreviousStarCount).ToString()); Previous version; Putting here in case current doesnt work

                    switch (StarCount)
                    {
                        case 120:
                            ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, should have just now finished Super Mario 64. If their starcount is not at 120 after a minute, please correct their total. This message was made by a bot.");
                            break;
                        case 241:
                            ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, should have just now started Super Mario Sunshine. If their starcount is not at 241 after a minute, please correct their total. This message was made by a bot.");
                            break;
                        case 360:
                            ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, should have just now finished Super Mario Sunshine. If their starcount is not at 360 after a minute, please correct their total. This message was made by a bot.");
                            break;
                        case 601:
                            ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, is now on The Perfect Run. If their starcount is not at 601 after a minute, please correct their total. They will manually enter the last star. This message was made by a bot.");
                            break;
                        default:
                            break;
                    }
                    /*if (StarCount == 120)   //Switched from if/else to switch/case. Keeping for future reference.
                    {
                        ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, should have just now finished Super Mario 64. If their starcount is not at 120 after a minute, please correct their total. This message was made by a bot.");
                    } else if(StarCount == 240)
                    {
                        ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, should have just now finished Super Mario Galaxy. If their starcount is not at 240 after a minute, please correct their total. This message was made by a bot.");
                    } else if(StarCount == 360)
                    {
                        ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, should have just now finished Super Mario Sunshine. If their starcount is not at 360 after a minute, please correct their total. This message was made by a bot.");
                    } else if(StarCount == 601)
                    {
                        ircClient.SendPublicChatMessage(_username + ", based on their livesplit progress, is now on The Perfect Run. If their starcount is not at 601 after a minute, please correct their total. They will manually enter the last star. This message was made by a bot.");
                    }*/
                }
                PreviousStarCount = StarCount;

                /*if(SplitID != "!!!!") //Old if statement from SMR program
                {
                    CurrentProgress = SplitID;
                }*/

                //Console.WriteLine("Current Progress" + CurrentProgress);
                //ircClient.SendPublicChatMessage(CurrentProgress);

                await Task.Delay(1500); //1.5 second break between messages for performance reasons
            }
        }
        
    }
}
