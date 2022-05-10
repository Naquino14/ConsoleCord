using System.Text;

namespace ConsoleCord
{
    /// <summary>
    /// Encoding Helper
    /// </summary>
    public class EncodingHelper
    {
        /// <summary>
        /// Byte array to string.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string B2S(byte[] a) => Encoding.ASCII.GetString(a);
        /// <summary>
        /// String to byte array.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static byte[] S2B(string a) => Encoding.ASCII.GetBytes(a);

    }
}
