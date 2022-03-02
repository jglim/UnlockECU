using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// CRD3 Level 1
    /// Originally written by Aloulou, and raised by Wilhelm Sorban (@WSorban) at https://github.com/jglim/UnlockECU/issues/15
    /// Unofficially named PowertrainSecurityAlgo3 as CRD ECU series typically use the PowertrainSecurityAlgo name prefix
    ///    
    /// There are no configurable parameters for this security provider
    /// </summary>
    class PowertrainSecurityAlgo3 : SecurityProvider
    {
        private static readonly uint FinalXorKey = 0x40088C88;
        private static readonly int[] SourceBitPositions = { 3, 7, 10, 11, 15, 19, 30 };
        private static readonly uint[] LookupTable =
        {
            0x45D145D1, 0x406E47C6, 0x5450C446, 0x51EFC651, 0x47CE507A, 0x4271526D, 0x3121A3DA, 0x349EA1CD,
            0x0CECABBF, 0x0953A9A8, 0x105B10DF, 0x15E412C8, 0x3E1D91A6, 0x3BA293B1, 0xA316122F, 0xA6A91038,
            0x545044C6, 0x51EF46D1, 0x45D1C551, 0x406EC746, 0x3121235A, 0x349E214D, 0x47CED0FA, 0x4271D2ED,
            0x105B905F, 0x15E49248, 0x0CEC2B3F, 0x09532928, 0xA31692AF, 0xA6A990B8, 0x3E1D1126, 0x3BA21331,
            0xC4CE5450, 0xA54D2082, 0xD54FD5C7, 0xD3A2D322, 0xC6D141FB, 0xA7523529, 0xB03EB25B, 0xB6D3B4BE,
            0xD921E349, 0x884CB829, 0x442A60C0, 0x94FB0349, 0xEBD0D950, 0xBABD8230, 0xF7676230, 0x27B601B9,
            0xD54F5547, 0xD3A253A2, 0xC4CED4D0, 0xA54DA002, 0xB03E32DB, 0xB6D3343E, 0xC6D1C17B, 0xA752B5A9,
            0x442AE040, 0x94FB83C9, 0xD92163C9, 0x884C38A9, 0xF767E2B0, 0x27B68139, 0xEBD059D0, 0xBABD02B0,
            0xE3BF18F8, 0xDDA62A01, 0x9550EB58, 0xAB49D9A1, 0xE1A00D53, 0xDFB93FAA, 0xF0218CC4, 0xCE38BE3D,
            0xA1CAE9CA, 0x9FD3DB33, 0x3CC16A43, 0x02D858BA, 0x933BD3D3, 0xAD22E12A, 0x8F8C68B3, 0xB1955A4A,
            0x95506BD8, 0xAB495921, 0xE3BF9878, 0xDDA6AA81, 0xF0210C44, 0xCE383EBD, 0xE1A08DD3, 0xDFB9BF2A,
            0x3CC1EAC3, 0x02D8D83A, 0xA1CA694A, 0x9FD35BB3, 0x8F8CE833, 0xB195DACA, 0x933B5353, 0xAD2261AA,
            0x5A4815E4, 0x5CB8A6A1, 0x4BC99473, 0x4D392736, 0x5857004F, 0x5EA7B30A, 0x2EB8F3EF, 0x284840AA,
            0x103A4AD8, 0x16CAF99D, 0x0C8DF1B8, 0x0A7D42FD, 0x22CB70C1, 0x243BC384, 0xBFC0F348, 0xB930400D,
            0x4BC914F3, 0x4D39A7B6, 0x5A489564, 0x5CB82621, 0x2EB8736F, 0x2848C02A, 0x585780CF, 0x5EA7338A,
            0x0C8D7138, 0x0A7DC27D, 0x103ACA58, 0x16CA791D, 0xBFC073C8, 0xB930C08D, 0x22CBF041, 0x243B4304,
        };


        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }
            uint seed = BytesToInt(inSeed, Endian.Big);
            
            // Remap 31-bits of seed into 7 bits, spanning from 0 to 0x7F/127 (both inclusive)
            uint remappedValue = 0;
            for (int i = 0; i < SourceBitPositions.Length; i++) 
            {
                remappedValue |= RemapBit(seed, SourceBitPositions[i], i);
            }

            uint key = seed ^ (seed & FinalXorKey) ^ LookupTable[remappedValue];

            IntToBytes(key, outKey, Endian.Big);
            return true;
        }

        public static uint RemapBit(uint inValue, int sourceBitPosition, int destinationBitPosition) 
        {
            return (uint)((inValue & (1 << sourceBitPosition)) > 0 ? (1 << destinationBitPosition) : 0);
        }

        public override string GetProviderName()
        {
            return "PowertrainSecurityAlgo3";
        }
    }
}

