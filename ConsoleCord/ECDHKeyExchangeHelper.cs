using ConsoleCord;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ADIS
{
    internal class ECDHKeyExchangeHelper
    {
        /// <summary>
        /// For server use.
        /// </summary>
        /// <returns></returns>
        internal static CngKey DerivePublicKey() // do it wokr>?>??>/
        {
            using (ECDiffieHellmanCng master = new())
            {
                master.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash; // ok
                master.HashAlgorithm = CngAlgorithm.Sha256; // there has to be a way to change this
                return CngKey.Import(master.PublicKey.ToByteArray(), CngKeyBlobFormat.EccPublicBlob);
            }
        }

        internal static byte[] DeriveSharedSecretKey() // i realize now idk what the fuck im doing
        {
            using (ECDiffieHellmanCng home = new())
            {
                //CngKey awayKey = CngKey.Import(awayPublicKeyStream, CngKeyBlobFormat.EccPublicBlob);
                throw new NotImplementedException(); // TODO: this
            }
        }
    }
}