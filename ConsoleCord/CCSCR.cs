using System;
using c = System.Console;
using sv = ConsoleCord.ConsoleCordServer;
using ADIS;
using ADIS.TLS;
using System.Net;
using System.Net.Sockets;

namespace ConsoleCord
{
    public class CCSCR
    {
        /// <summary>
        /// Should only be 1 reference.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="state"></param>
        public static void RegisterCommand(ADISCommand command, SvClient client)
        {
            switch (command.instruction)
            {
                case ADISinstruction.nul:
                    ;
                    break;
                case ADISinstruction.echo:
                    Echo(command, client.ClientSocket);
                    break;
                case ADISinstruction.clhello:
                    HandleClHello(command, client);
                    break;
            }
        }

        public static void RegisterCommand(CCCommand command, object? state = null)
        {
            throw new NotImplementedException();
        }

        private static void HandleClHello(ADISCommand command, object? state)
        {
            SvClient client = (SvClient)state!;
            Socket socket = client.ClientSocket;
            // register clhello and write the clients name to the arraylist.
            client.ClientName = command.args![0];
            c.WriteLine($"CLHello from client recieved.");
            sv.Clients[client.ClientNumber].ClientName = client.ClientName;
            c.WriteLine($"Client {client.ClientName}#{client.ClientNumber} has been registered.");

            // TODO: check versions of ADIS stuff
            // send svhello
            var args = new string[] { "this is a placeholder public key", sv.ServerName };
            ADISCommand svHello = new(ADISinstruction.svhello, args);
            c.WriteLine($"Responding with SVHELLO. {svHello}.");
            var packet = ADISCR.MarshalCommand(svHello);
            socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(sv.SendCallback), socket);
            socket.BeginReceive(sv.gBuffer, 0, sv.gBuffer.Length, SocketFlags.None, new AsyncCallback(sv.RecieveCallback), client);
        }

        private static void Echo(ADISCommand command, Socket socket)
        {
            // call the command registry to begin the handshake process.
            c.WriteLine("Responding...");
            // testing
            if (command.instruction == ADISinstruction.echo)
            {
                string concatArgs = "";
                if (command.args is not null && command.args.Length > 1)
                    foreach (var s in command.args)
                        concatArgs += $"{s} ";
                var packet = EH.S2B(command.args is not null ? concatArgs : "I cant echo nothing silly!");
                socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(sv.SendCallback), socket);
            }
        }
    }
}
