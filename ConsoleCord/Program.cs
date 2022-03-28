//#define DEBUG_ARGS

using System;
using c = System.Console;
using ADIS;

namespace ConsoleCord
{
    public class Program
    {
        public static void Main(string[] args)
        {
            args[0] = args[0].Contains('/') ? args[0].Remove(args[0].Length - 1, 1).Remove(0, 7) : args[0].Contains('-') && !args[0].Contains(':') ? args[0] : args[0].Remove(0, 5);
            string[] subArguments = args[0].Split("%20");
            // ok so basically
            // if ur running an argument, dont remove anything
            // if it looks like cctp://command, get rid of anything befo command and that one nasty extraneous \ that exists for some reason...
            // or if it looks like cctp:command, same thing

            /// Argument structure for servers:
            /// cctp://server ip
            /// port is optional
            switch (subArguments[0])
            {
                case "--setupProtocol":
                    ProtocolManager.Elevate(ProtocolManager.InstallType.install);
                    break;
                case "--setupProtocolAdmin":
                    ProtocolManager.InstallProtocol();
                    break;
                case "--removeProtocol":
                    ProtocolManager.Elevate(ProtocolManager.InstallType.remove);
                    break;
                case "--removeProtocolAdmin":
                    ProtocolManager.UninstallProtocol();
                    break;
                case "server":
                    var server = new ConsolecordServer(int.Parse(subArguments[1]), subArguments[2]);
                    c.WriteLine("Press any key to stop the server.");
                    c.ReadLine();
                    break;
                case "client":
                    string clientName = subArguments.Length >= 3 ? subArguments[2] : Environment.MachineName;
                    var client = new ConsoleCordClient(int.Parse(subArguments[1]), clientName);
                    c.WriteLine("Press any key to stop debugging.");
                    break;
                case "hello":
                    c.WriteLine("Hey There!");
                    c.ReadLine();
                    break;
                case "echo":
                    var fo = true;
                    foreach (var a in subArguments)
                    {
                        if (fo)
                        { fo = !fo; continue; }
                        c.Write($"{a} ");
                    }
                    break;
                //default:
                //    c.WriteLine($"Invalid argument(s): {(args.Length == 0 ? "no arguments" : $"{args[0]}")} Press any key to continue.");
                //    c.ReadLine();
                //    return;
            }
            #if DEBUG_ARGS
            foreach (var s in subArguments)
                c.WriteLine(s);
            #endif
            c.WriteLine("\nPress any key to coninue.");
            c.ReadKey();
        }
    }
}