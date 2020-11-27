using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Utilities for bit and byte operations.
    /// (Frequently copied-and-pasted across my projects)
    /// </summary>
    public class BitUtility
    {
        /// <summary>
        /// Sets all values in an array of bytes to a specific value
        /// </summary>
        /// <param name="value">Value to set byte array to</param>
        /// <param name="buf">Target byte array buffer</param>
        public static void Memset(byte value, byte[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = value;
            }
        }
        // Internally used by BytesFromHex
        private static byte[] StringToByteArrayFastest(string hex)
        {
            // see https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
            if (hex.Length % 2 == 1)
            {
                throw new Exception("The binary key cannot have an odd number of digits");
            }
            byte[] arr = new byte[hex.Length >> 1];
            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexValue(hex[i << 1]) << 4) + (GetHexValue(hex[(i << 1) + 1])));
            }
            return arr;
        }
        // Internally used by StringToByteArrayFastest
        private static int GetHexValue(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }
        /// <summary>
        /// Converts an array of bytes into its hex-string equivalent
        /// </summary>
        /// <param name="inBytes">Input byte array</param>
        /// <param name="spacedOut">Option to add spaces between individual bytes</param>
        /// <returns>Hex-string based on the input byte array</returns>
        public static string BytesToHex(byte[] inBytes, bool spacedOut = false)
        {
            return BitConverter.ToString(inBytes).Replace("-", spacedOut ? " " : "");
        }

        /// <summary>
        /// Converts an array of bytes into a printable hex-string
        /// </summary>
        /// <param name="hexString">Input hex-string to convert into a byte array</param>
        /// <returns>Byte array based on the input hex-string</returns>
        public static byte[] BytesFromHex(string hexString)
        {
            return StringToByteArrayFastest(hexString.Replace(" ", ""));
        }

        /// <summary>
        /// Resize a smaller array of bytes to a larger array. The padding bytes will be 0.
        /// </summary>
        /// <param name="inData">Input byte array</param>
        /// <param name="finalSize">New size for the input array</param>
        /// <returns>Resized byte array</returns>
        public static byte[] PadBytes(byte[] inData, int finalSize)
        {
            if (inData.Length > finalSize)
            {
                return inData;
            }
            byte[] result = new byte[finalSize];
            Buffer.BlockCopy(inData, 0, result, 0, inData.Length);
            return result;
        }

    }
}
