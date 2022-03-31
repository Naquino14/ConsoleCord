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
                    break;
                case ADISinstruction.echo:
                    Echo(command, client);
                    break;
                case ADISinstruction.clhello:
                    HandleClHello(command, client);
                    break;
                case ADISinstruction.reqKey:
                    client.incomingType = ExpectedRawType.trust;
                    SendKey(client);
                    break;
                case ADISinstruction.reqIV:
                    SendPacket(client.CurrentIV, client);
                    break;
            }
        }

        public static void RegisterCommand(CCCommand command, SvClient client)
        {
            throw new NotImplementedException();
        }

        private static void HandleClHello(ADISCommand command, SvClient client)
        {
            Socket socket = client.ClientSocket;
            // register clhello and write the clients name to the arraylist.
            client.ClientName = command.args![0];
            c.WriteLine($"CLHello from client recieved.");
            sv.Clients[client.ClientNumber].ClientName = client.ClientName;
            c.WriteLine($"Hello {client.ClientName}! You have been registered as #{client.ClientNumber}.");

            // TODO: check versions of ADIS stuff
            // send svhello
            HandshakeHelper.HandshakeSVHello(client);
        }

        private static void SendKey(SvClient client)
        {
            client.incomingType = ExpectedRawType.key;
            c.WriteLine($"Sending client {client.ClientName} its respective public key...\nListening for client key.");
            SendPacket(client.svPublicKey, client);
        }

        public static void HandleEchoTrust(SvClient client, byte[] clientTrust)
        {
            c.WriteLine("Checking client echoTrust.");
            byte[] serverTrust = HandshakeHelper.OTPArray(
                HandshakeHelper.OTPArray(EH.S2B(client.EchoTrustArgs[0]), EH.S2B(client.EchoTrustArgs[2])),
                HandshakeHelper.OTPArray(EH.S2B(client.EchoTrustArgs[1]), EH.S2B(client.EchoTrustArgs[3])));
            // decrypt client trust
            clientTrust = HandshakeHelper.DecryptPayload(clientTrust, client);
            if (Enumerable.SequenceEqual(serverTrust, clientTrust))
            {
                client.Secured = true;
                ADISCommand handshakeOk = new(ADISinstruction.handshakeOk);
                var packet = ADISCR.MarshalCommand(handshakeOk);
                SendPacket(packet, client);
            }
            else
            {
                var args = new string[1] { "Could not establsih a secure connection between the server and the client." };
                ADISCommand cutCom = new(ADISinstruction.cutCom, args);
                var packet = ADISCR.MarshalCommand(cutCom);
                SendPacket(packet, client);
            }

        }

        private static void Echo(ADISCommand command, SvClient client)
        {
            // call the command registry to begin the handshake process.
            c.WriteLine("Responding to echo...");
            // testing
            if (command.instruction == ADISinstruction.echo)
            {
                string concatArgs = "";
                if (command.args is not null && command.args.Length > 1)
                    foreach (var s in command.args)
                        concatArgs += $"{s} ";
                var packet = EH.S2B(command.args is not null ? concatArgs : "I cant echo nothing silly!");
                SendPacket(packet, client);
            }
        }

        internal static void SendPacket(byte[] packet, SvClient client)
        {
            client.ClientSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(sv.SendCallback), client);
            client.ClientSocket.BeginReceive(sv.gBuffer, 0, sv.gBuffer.Length, SocketFlags.None, new AsyncCallback(sv.RecieveCallback), client);
        }
    }
}
