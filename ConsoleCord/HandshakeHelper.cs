using ConsoleCord;
using sv = ConsoleCord.ConsoleCordServer;
using cc = ConsoleCord.ConsoleCordClient;
using System;
using c = System.Console;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Sockets;
using ADIS.TLS;

namespace ADIS
{
    internal class HandshakeHelper
    {
        // keys are sent after svhello or clhello and shouldnt be demarshalled

        public static void HandshakeSVHello(SvClient client)
        {
            c.WriteLine("Responding to ClHello...");
            ECDiffieHellmanCng server = new();
            var args = new string[1] { sv.ServerName };
            client.Curve = server;
            server.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            server.HashAlgorithm = CngAlgorithm.Sha256;
            var serverPubKey = server.PublicKey.ToByteArray();
            client.svPublicKey = serverPubKey;
            ADISCommand svHello = new(ADISinstruction.svhello, args);
            var packet = ADISCR.MarshalCommand(svHello);
            c.WriteLine($"Sending SVHello to client. {svHello}");
            CCSCR.SendPacket(packet, client);
        }

        public static void HandshakeSvEchoTrust(byte[] clientPublicKey, SvClient client)
        {
            c.Write("Public key from client recieved: ");
            foreach (var b in clientPublicKey)
                c.Write($"{b:X}");
            c.WriteLine($"\nWith a length of {clientPublicKey.Length}.");
            c.WriteLine("Generating shared private key...");
            CngKey clientCng = CngKey.Import(clientPublicKey, CngKeyBlobFormat.EccPublicBlob);
            client.sharedPrivateKey = client.Curve.DeriveKeyMaterial(clientCng);
            client.Curve.Dispose();
            c.Write($"Key for client {client.ClientName}: ");
            foreach (var b in client.sharedPrivateKey)
                c.Write($"{b:X}");
            c.WriteLine($"\nWith a key size of {client.sharedPrivateKey.Length}");
            c.WriteLine("Compressing key...");
            client.sharedPrivateKey = SquishKey(client.sharedPrivateKey);
            // TODO: call encryptor, decrypt, and respond
            c.WriteLine("Encrypting secure echo request...");
            // random string generator
            // ascii range is from (0->93)+33
            var random = new Random();
            string[] args = new string[4];
            for (int j = 0; j < 4; j++)
            {
                var chars = new byte[16];
                for (int i = 0; i < 16; i++)
                    chars[i] = (byte)random.Next(33, 93);
                args[j] = EH.B2S(chars);
            }
            ADISCommand echotrust = new(ADISinstruction.echoTrust, args);
            var packet = ADISCR.MarshalCommand(echotrust);
            c.WriteLine($"echoTrust formed: {echotrust}.");
            c.WriteLine($"Raw echotrust stream:\n{EH.B2S(packet)}");

            c.WriteLine("Encrypting echoTrust...");
            // encryption
            packet = EncryptPacket(packet, client.sharedPrivateKey, client);
            c.WriteLine($"Sending encrypted echoTrust and awaiting response... (Packet size is {packet.Length} bytes)");
            client.EchoTrustArgs = args;
            CCSCR.SendPacket(packet, client);
        }

        /// <summary>
        /// Encrypts a packet and sets the current IV for the client.
        /// </summary>
        /// <param name="insecurePacket">The packet to secure using ARC-128.</param>
        /// <param name="sharedPrivateKey">The shared private key between the client and the server.</param>
        /// <param name="client">The client.</param>
        /// <returns>The packet secured with ARC-128.</returns>
        public static byte[] EncryptPacket(byte[] insecurePacket, byte[] sharedPrivateKey, SvClient client)
        {
            ARC128 arc = new();
            client.CurrentIV = arc.GenerateIV();
            return arc.Encrypt(insecurePacket, sharedPrivateKey, client.CurrentIV);
        }

        public static byte[] DecryptPayload(byte[] payload, SvClient client) => new ARC128(payload, client.sharedPrivateKey, client.CurrentIV).Decrypt();

        public static byte[] SquishKey(byte[] key)
        {
            byte[] subkey1 = new byte[16], 
                subkey2 = new byte[16];
            Array.Copy(key, subkey1, 16);
            Array.Copy(key, 16, subkey2, 0, 16);
            return OTPArray(subkey1, subkey2);
        }

        public static byte[] OTPArray(byte[] input, byte[] key)
        {
            byte[] result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[i] = (byte)(input[i] ^ key[i]);
            return result;
        }
    }
}