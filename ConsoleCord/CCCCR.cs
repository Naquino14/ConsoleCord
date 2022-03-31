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
                    break;
                case ADISinstruction.clhello:
                    
                    break;
            }
        }

        public static void RegisterCommand(CCCommand command, object? state = null)
        {
            switch (command.instruction)
            {
                case CCInstruction.nul:
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

        public static void SendCommand(ADISCommand command)
        {
            c.WriteLine($"Sending command: {command}.");
            byte[] packet = ADISCR.MarshalCommand(command);
            cc.ClientSocket.Send(packet);
        }

        public static void SendPacket(byte[] packet) => cc.ClientSocket.Send(packet);
    }
}
