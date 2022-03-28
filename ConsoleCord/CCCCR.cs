using System;
using c = System.Console;
using cc = ConsoleCord.ConsoleCordClient;
using ADIS;
using ADIS.TLS;
using System.Net;
using System.Net.Sockets;

namespace ConsoleCord
{
    /// <summary>
    /// ConsoleCord Client Command Registry.
    /// </summary>
    public class CCCCR
    {
        public static void RegisterCommand(ADISCommand command, object? state = null)
        {
            switch (command.instruction)
            {
                case ADISinstruction.nul:
                    ;
                    break;
                case ADISinstruction.clhello:
                    CheckSVHello(command);
                    break;
            }
        }

        public static void RegisterCommand(CCCommand command, object? state = null)
        {
            switch (command.instruction)
            {
                case CCInstruction.nul:
                    ;
                    break;
            }
        }

        public static void RefuseConnection(string msg)
        {
            ADISCommand cutCom = new(ADISinstruction.cutCom, new string[] { $"Connection refused by client: {msg}" });
            var payload = ADISCR.MarshalCommand(cutCom);
            cc.ClientSocket.Send(payload);
            cc.ClientSocket.Shutdown(SocketShutdown.Both);
            cc.ClientSocket.Close();
            cc.ClientSocket.Dispose();
            Environment.Exit(-2);
        }

        public static void CheckSVHello(ADISCommand svHello)
        {
            // structure: svhello sessionName marshalledpublicKey 
            if (svHello.instruction != ADISinstruction.svhello)
            { c.WriteLine($"SVHello invalid instruction: {svHello.instruction}"); RefuseConnection("Invalid instruction during handshake."); }
            if (svHello.args is null || svHello.args.Length != 2)
            { c.WriteLine($"SVHello invalid arg count. Server provided {(svHello.args is not null ? svHello.args.Length : 0)} arguments."); RefuseConnection("Invalid instructuion during handshake."); }
            if (svHello.args![0].ToLower() == cc.SessionName.ToLower())
            { c.WriteLine($"SVhello invalid. Server name mismatch: Expected {cc.SessionName} and recieved {svHello.args[0]}"); RefuseConnection("Invalid instructuion during handshake."); }
        }

        public static void SendCommand(ADISCommand command)
        {
            c.WriteLine($"Sending command: {command}.");
            byte[] packet = ADISCR.MarshalCommand(command);
            cc.ClientSocket.Send(packet);
        }
    }
}
