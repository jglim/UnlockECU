using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Basic SecurityProvider to be inherited from. This class should not be directly initialized.
    /// </summary>
    public class SecurityProvider
    {
        public virtual string GetProviderName() 
        {
            return "ProviderName was not initialized";
        }

        public virtual bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters) 
        {
            throw new Exception("GenerateKey was not overridden");
        }

        public static byte GetParameterByte(List<Parameter> parameters, string key)
        {
            foreach (Parameter row in parameters)
            {
                if ((row.Key == key) && (row.DataType == "Byte"))
                {
                    return (byte)(int.Parse(row.Value, System.Globalization.NumberStyles.HexNumber));
                }
            }
            throw new Exception($"Failed to fetch byte parameter for key: {key}");
        }
        public static int GetParameterInteger(List<Parameter> parameters, string key)
        {
            foreach (Parameter row in parameters)
            {
                if ((row.Key == key) && (row.DataType == "Int32"))
                {
                    return int.Parse(row.Value, System.Globalization.NumberStyles.HexNumber);
                }
            }
            throw new Exception($"Failed to fetch Int32 parameter for key: {key}");
        }
        public static long GetParameterLong(List<Parameter> parameters, string key)
        {
            foreach (Parameter row in parameters)
            {
                if ((row.Key == key) && (row.DataType == "Int64"))
                {
                    return long.Parse(row.Value, System.Globalization.NumberStyles.HexNumber);
                }
            }
            throw new Exception($"Failed to fetch Int64 parameter for key: {key}");
        }
        public static byte[] GetParameterBytearray(List<Parameter> parameters, string key)
        {
            foreach (Parameter row in parameters)
            {
                if ((row.Key == key) && (row.DataType == "ByteArray"))
                {
                    return BitUtility.BytesFromHex(row.Value);
                }
            }
            throw new Exception($"Failed to fetch ByteArray parameter for key: {key}");
        }

        private static bool IsInitialized = false;
        private static List<SecurityProvider> SecurityProviders = new();

        public static List<SecurityProvider> GetSecurityProviders()
        {
            if (IsInitialized)
            {
                return SecurityProviders;
            }
            SecurityProviders = new List<SecurityProvider>();

            System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsSubclassOf(typeof(SecurityProvider)))
            .ToList()
            .ForEach(x => SecurityProviders.Add((SecurityProvider)Activator.CreateInstance(x)));
            IsInitialized = true;

            return SecurityProviders;
        }

        public enum Endian 
        {
            Big,
            Little,
        }

        public static uint BytesToInt(byte[] inBytes, Endian endian, int offset = 0)
        {
            uint result = 0;
            if (endian == Endian.Big)
            {
                result |= (uint)inBytes[offset++] << 24;
                result |= (uint)inBytes[offset++] << 16;
                result |= (uint)inBytes[offset++] << 8;
                result |= (uint)inBytes[offset++] << 0;
            }
            else
            {
                result |= (uint)inBytes[offset++] << 0;
                result |= (uint)inBytes[offset++] << 8;
                result |= (uint)inBytes[offset++] << 16;
                result |= (uint)inBytes[offset++] << 24;
            }
            return result;
        }
        public static void IntToBytes(uint inInt, byte[] outBytes, Endian endian)
        {
            if (endian == Endian.Big)
            {
                outBytes[0] = (byte)(inInt >> 24);
                outBytes[1] = (byte)(inInt >> 16);
                outBytes[2] = (byte)(inInt >> 8);
                outBytes[3] = (byte)(inInt >> 0);
            }
            else
            {
                outBytes[3] = (byte)(inInt >> 24);
                outBytes[2] = (byte)(inInt >> 16);
                outBytes[1] = (byte)(inInt >> 8);
                outBytes[0] = (byte)(inInt >> 0);
            }
        }

        // WARNING: endian unaware:
        public static byte GetBit(byte inByte, int bitPosition)
        {
            if (bitPosition > 7)
            {
                throw new Exception("Attempted to shift beyond 8 bits in a byte");
            }
            return (byte)((inByte >> bitPosition) & 1);
        }

        public static byte GetByte(uint inInt, int bytePosition)
        {
            if (bytePosition > 3)
            {
                throw new Exception("Attempted to shift beyond 4 bytes in an uint");
            }
            return (byte)(inInt >> (8 * bytePosition));
        }

        public static byte SetBit(byte inByte, int bitPosition)
        {
            if (bitPosition > 7)
            {
                throw new Exception("Attempted to shift beyond 8 bits in a byte");
            }
            return inByte |= (byte)(1 << bitPosition);
        }

        public static uint SetByte(uint inInt, byte byteToSet, int bytePosition)
        {
            if (bytePosition > 3)
            {
                throw new Exception("Attempted to shift beyond 4 bytes in an uint");
            }
            int bitPosition = 8 * bytePosition;
            inInt &= ~(uint)(0xFF << bitPosition);
            inInt |= (uint)(byteToSet << bitPosition);
            return inInt;
        }

        public static byte[] ExpandByteArrayToNibbles(byte[] inputArray)
        {
            // Primarily used for IC172
            byte[] result = new byte[inputArray.Length * 2];
            for (int i = 0; i < inputArray.Length; i++)
            {
                result[i * 2] = (byte)((inputArray[i] >> 4) & 0xF);
                result[i * 2 + 1] = (byte)(inputArray[i] & 0xF);
            }
            return result;
        }

        public static byte[] CollapseByteArrayFromNibbles(byte[] inputArray)
        {
            // Primarily used for IC172
            if ((inputArray.Length % 2) != 0)
            {
                throw new Exception("Attempted to form a byte array from an odd-numbered set of nibbles.");
            }
            byte[] result = new byte[inputArray.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)((inputArray[i * 2] << 4) | (inputArray[i * 2 + 1]));
            }
            return result;
        }

    }
}
