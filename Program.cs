using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace Velonabot
{
    
    class Program
    {
        private static string _user;
        private static string _oauth;
        private static string _channel;

        public static string[] moderators = { "thestibblr", "jakecn", "dnvic", "velonathon", "bgp1", "dnvicalt" };

        public static List<CustomCommands> CommandsList = new List<CustomCommands>();
        public static List<CustomCommands> ModeratorCommandsList = new List<CustomCommands>();
        public static List<CustomCommands> UserCommandsList = new List<CustomCommands>();

        private static string CommandLocation = @"C:\Velonathon\BotCommands\Commands.json";

        private static void assignStrings()
        {
            LoginCredentials loginCredentials = new LoginCredentials();

            try
            {
                loginCredentials = LoginCredentials.GetCredentials(@"C:\Velonathon\credentials.json");
            } 
            catch
            {
                Console.Write("Insert bot username ");
                loginCredentials.user = Console.ReadLine();

                Console.Write("Insert bot oauth ID ");
                loginCredentials.oauth = Console.ReadLine();

                Console.Write("Insert channel to autocount in ");
                loginCredentials.channel = Console.ReadLine();

                LoginCredentials.SaveCredentials(loginCredentials, @"C:\Velonathon\credentials.json");
            }

            


            _user = loginCredentials.user;
            _oauth = loginCredentials.oauth;
            _channel = loginCredentials.channel;
        }

        static async Task Main(string[] args)
        {
            assignStrings();
            await Task.WhenAll(LoadCommands(), ExecuteClient());
        }
        static async Task ExecuteClient()
        {
            
            IrcClient ircClient = new IrcClient("irc.chat.twitch.tv", 6667, _user, _oauth, _channel);
            PingSender ping = new PingSender(ircClient);
            while (true)
            {
                string message = ircClient.ReadMessage();
                Console.WriteLine(message);


                if (message.Contains("PRIVMSG"))
                {
                    int intIndexParseSign = message.IndexOf('!');
                    string userName = message.Substring(1, intIndexParseSign - 1);
                    intIndexParseSign = message.IndexOf(" :");
                    message = message.Substring(intIndexParseSign + 2);

                    //Mod Commands
                    if (moderators.Contains(userName))
                    {
                        await ModeratorCommands(userName, message, ircClient);
                    }
                    UserCommands(userName, message, ircClient);


                    

                }
                //await Task.Delay(0);
            }
        }
        static async Task ModeratorCommands(string userName, string message, IrcClient irc)
        {
            if (message.StartsWith("!hello"))
            {
                irc.SendPublicChatMessage("World!");
            }
            if (message.StartsWith("!addcom"))
            {
                var fmtstring = message.Substring(8);
                var FirstSpaceIndex = fmtstring.IndexOf(' ');
                var FirstString = fmtstring.Substring(0, FirstSpaceIndex);
                foreach (CustomCommands com in CommandsList)
                {
                    if (FirstString == com.CommandName)
                    {
                        irc.SendPublicChatMessage("That command already exists. Try again");
                        goto End; // Ends the if statement if the command already exists.
                    }
                }
                var SecondString = fmtstring.Substring(FirstSpaceIndex + 1);
                CustomCommands command = new CustomCommands();
                command.CommandName = FirstString;
                command.CommandResponse = SecondString;
                command.IsModCommand = false;
                CommandsList.Add(command);
                CustomCommands.SaveCommands(CommandsList, CommandLocation);
                irc.SendPublicChatMessage("Command added!");
                End:;
            }
            if (message.StartsWith("!delcom"))
            {
                var fmtstring = message.Substring(8);
                var foundCommand = false;
                foreach(CustomCommands command in CommandsList)
                {

                    if(command.CommandName == fmtstring)
                    {
                        foundCommand = true;
                        CommandsList.Remove(command);
                        CustomCommands.SaveCommands(CommandsList, CommandLocation);
                        irc.SendPublicChatMessage("Command Removed!");
                        break;
                    }
                }
                if(!foundCommand)
                {
                    irc.SendPublicChatMessage("No command with that name exists");
                }
            }
            if (message.StartsWith("!editcom"))
            {
                var fmtstring = message.Substring(9);
                var FirstSpaceIndex = fmtstring.IndexOf(' ');
                var FirstString = fmtstring.Substring(0, FirstSpaceIndex);
                var foundCommand = false;
                foreach (CustomCommands com in CommandsList)
                {
                    if (FirstString == com.CommandName)
                    {
                        var SecondString = fmtstring.Substring(FirstSpaceIndex + 1);
                        com.CommandResponse = SecondString;
                        CustomCommands.SaveCommands(CommandsList, CommandLocation);
                        foundCommand = true;
                        irc.SendPublicChatMessage("Command Edited!");
                        break;
                    }
                }
                if (!foundCommand)
                {
                    irc.SendPublicChatMessage("No command with that name exists");
                }
            }
        }
        static void UserCommands(string userName, string message, IrcClient irc)
        {
            if (message.StartsWith("!hola"))
            {
                irc.SendPublicChatMessage("Mundo!");
            }
            if (message.StartsWith("!commands"))
            {
                List<string> Commands = new List<string>();
                foreach(CustomCommands command in CommandsList)
                {
                    Commands.Add(command.CommandName);
                }
                string finalmsg = "The current commands are:";
                for(var i = 0; i < Commands.Count; i++)
                {
                    finalmsg += " ";
                    if(i == Commands.Count - 1)
                    {
                        finalmsg += Commands[i];
                    } else
                    {
                        finalmsg += Commands[i];
                        finalmsg += ",";
                    }
                    
                }
                irc.SendPublicChatMessage(finalmsg);
            }
            foreach(CustomCommands command in UserCommandsList)
            {
                if(message.StartsWith(command.CommandName))
                {
                    string finalmsg = command.CommandResponse;
                    if(finalmsg.Contains("$USER"))
                    {
                        finalmsg = finalmsg.Replace("$USER", userName);
                        Console.WriteLine(userName);
                    }
                    if (finalmsg.Contains("$COUNTER"))
                    {
                        finalmsg = finalmsg.Replace("$COUNTER", (command.Counter + 1).ToString());
                        command.Counter += 1;
                        CustomCommands.SaveCommands(CommandsList, CommandLocation);
                    }
                    Console.WriteLine(finalmsg);
                    irc.SendPublicChatMessage(finalmsg);
                    break;
                }
            }

        }
        static async Task LoadCommands()
        {
            while(true)
            {
                CommandsList = CustomCommands.LoadCommands(CommandLocation);
                for( int i = 0; i < CommandsList.Count; i++)
                {
                    if(CommandsList[i].IsModCommand)
                    {
                        ModeratorCommandsList.Add(CommandsList[i]);
                    } 
                    else
                    {
                        UserCommandsList.Add(CommandsList[i]);
                    }
                }
                await Task.Delay(5000);
            }
            
        }
    }
}
