using System;
using c = System.Console;
using EH = ConsoleCord.EncodingHelper;
using ADIS;
using ADIS.TLS;

namespace ConsoleCord
{
    public class ServerCommandRegistry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="client"></param>
        /// <returns>A packet to be sent back to the client.</returns>
        public byte[] RegisterCommand(in byte[] payload, SvClient client)
        {
            byte[]? packet = null;
            // todo
            var parsedPayload = EH.B2S(payload);
            if (client.Secured)
            {

            }
            else
            {
                switch (parsedPayload.Split(' ')[0])
                {
                    case "echo":
                        packet = EH.S2B($"PUT \"{payload.Skip(parsedPayload.Split(' ')[0].Length + 1)}\"");
                        break;
                }
            }
            return packet ?? (client.Secured ? new ARC128().Encrypt(EH.S2B("NACK \"Parse Failed\"")) : EH.S2B("NACK \"Parse Failed\""));
        }
    }
}
