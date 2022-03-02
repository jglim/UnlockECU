using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Basic implementation PowertrainSecurityAlgo.
    /// </summary>
    class PowertrainSecurityAlgo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[][] matrix = new byte[][]
            {
                new byte[]{ GetParameterByte(parameters, "X00"), GetParameterByte(parameters, "X01"), GetParameterByte(parameters, "X02"), GetParameterByte(parameters, "X03") },
                new byte[]{ GetParameterByte(parameters, "X10"), GetParameterByte(parameters, "X11"), GetParameterByte(parameters, "X12"), GetParameterByte(parameters, "X13") },
                new byte[]{ GetParameterByte(parameters, "X20"), GetParameterByte(parameters, "X21"), GetParameterByte(parameters, "X22"), GetParameterByte(parameters, "X23") },
                new byte[]{ GetParameterByte(parameters, "X30"), GetParameterByte(parameters, "X31"), GetParameterByte(parameters, "X32"), GetParameterByte(parameters, "X33") },
                new byte[]{ GetParameterByte(parameters, "X40"), GetParameterByte(parameters, "X41"), GetParameterByte(parameters, "X42"), GetParameterByte(parameters, "X43") },
                new byte[]{ GetParameterByte(parameters, "X50"), GetParameterByte(parameters, "X51"), GetParameterByte(parameters, "X52"), GetParameterByte(parameters, "X53") },
                new byte[]{ GetParameterByte(parameters, "X60"), GetParameterByte(parameters, "X61"), GetParameterByte(parameters, "X62"), GetParameterByte(parameters, "X63") },
                new byte[]{ GetParameterByte(parameters, "X70"), GetParameterByte(parameters, "X71"), GetParameterByte(parameters, "X72"), GetParameterByte(parameters, "X73") },
            };

            int[] i = new int[]
            {
                GetParameterInteger(parameters, "i1"),
                GetParameterInteger(parameters, "i2"),
                GetParameterInteger(parameters, "i3"),
                GetParameterInteger(parameters, "i4"),
                GetParameterInteger(parameters, "i5"),
                GetParameterInteger(parameters, "i6")
            };
            int[] j = new int[]
            {
                GetParameterInteger(parameters, "j1"),
                GetParameterInteger(parameters, "j2"),
                GetParameterInteger(parameters, "j3"),
                GetParameterInteger(parameters, "j4"),
                GetParameterInteger(parameters, "j5"),
                GetParameterInteger(parameters, "j6")
            };

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            byte[] workingSeed = new byte[] { inSeed[3], inSeed[2], inSeed[1], inSeed[0] };

            byte y = (byte)(workingSeed[i[0]] ^ workingSeed[i[1]]);
            int dBit2 = GetBit(workingSeed[i[2]], j[0]);
            int dBit1 = GetBit(workingSeed[i[3]], j[1]);
            int dBit0 = GetBit(y, j[2]);
            uint dValue = CreateDValue(dBit2, dBit1, dBit0, matrix);

            uint seedAsInt = BytesToInt(workingSeed, Endian.Little);

            uint dXorIntermediate = seedAsInt ^ dValue;
            int gBit2 = GetBit(workingSeed[i[4]], j[3]);
            int gBit1 = GetBit(y, j[4]);
            int gBit0 = GetBit(GetByte(dXorIntermediate, i[5]), j[5]);
            uint gValue = CreateGValue(gBit2, gBit1, gBit0, matrix);

            uint seedKey = dXorIntermediate ^ gValue;

            IntToBytes(seedKey, outKey, Endian.Big);
            return true;
        }

        private static uint CreateDValue(int bit2Enabled, int bit1Enabled, int bit0Enabled, byte[][] matrix)
        {
            uint i = 0;
            byte j = 0;
            if (bit0Enabled != 0)
            {
                j = SetBit(j, 0);
            }
            if (bit1Enabled != 0)
            {
                j = SetBit(j, 1);
            }
            if (bit2Enabled != 0)
            {
                j = SetBit(j, 2);
            }
            i = SetByte(i, matrix[j][3], 0);
            i = SetByte(i, matrix[j][2], 1);
            i = SetByte(i, matrix[j][1], 2);
            i = SetByte(i, matrix[j][0], 3);
            return i;
        }

        private static uint CreateGValue(int bit2Enabled, int bit1Enabled, int bit0Enabled, byte[][] matrix)
        {
            uint i = 0;
            byte j = 0;
            if (bit0Enabled != 0)
            {
                j = SetBit(j, 0);
            }
            if (bit1Enabled != 0)
            {
                j = SetBit(j, 1);
            }
            if (bit2Enabled != 0)
            {
                j = SetBit(j, 2);
            }
            i = SetByte(i, matrix[j][2], 0);
            i = SetByte(i, matrix[j][1], 1);
            i = SetByte(i, matrix[j][0], 2);
            i = SetByte(i, matrix[j][3], 3);
            return i;
        }

        public override string GetProviderName()
        {
            return "PowertrainSecurityAlgo";
        }
    }
}
