using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Derivative of PowertrainSecurityAlgo, with a slightly different byte ordering when computing the g-value.
    /// </summary>
    class PowertrainSecurityAlgo2 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[][] matrix = new byte[][]
            {
                new byte[]{ GetParameterByte(parameters, "XX00"), GetParameterByte(parameters, "XX01"), GetParameterByte(parameters, "XX02"), GetParameterByte(parameters, "XX03") },
                new byte[]{ GetParameterByte(parameters, "XX10"), GetParameterByte(parameters, "XX11"), GetParameterByte(parameters, "XX12"), GetParameterByte(parameters, "XX13") },
                new byte[]{ GetParameterByte(parameters, "XX20"), GetParameterByte(parameters, "XX21"), GetParameterByte(parameters, "XX22"), GetParameterByte(parameters, "XX23") },
                new byte[]{ GetParameterByte(parameters, "XX30"), GetParameterByte(parameters, "XX31"), GetParameterByte(parameters, "XX32"), GetParameterByte(parameters, "XX33") },
                new byte[]{ GetParameterByte(parameters, "XX40"), GetParameterByte(parameters, "XX41"), GetParameterByte(parameters, "XX42"), GetParameterByte(parameters, "XX43") },
                new byte[]{ GetParameterByte(parameters, "XX50"), GetParameterByte(parameters, "XX51"), GetParameterByte(parameters, "XX52"), GetParameterByte(parameters, "XX53") },
                new byte[]{ GetParameterByte(parameters, "XX60"), GetParameterByte(parameters, "XX61"), GetParameterByte(parameters, "XX62"), GetParameterByte(parameters, "XX63") },
                new byte[]{ GetParameterByte(parameters, "XX70"), GetParameterByte(parameters, "XX71"), GetParameterByte(parameters, "XX72"), GetParameterByte(parameters, "XX73") },
            };

            int[] i = new int[]
            {
                GetParameterInteger(parameters, "ii1"),
                GetParameterInteger(parameters, "ii2"),
                GetParameterInteger(parameters, "ii3"),
                GetParameterInteger(parameters, "ii4"),
                GetParameterInteger(parameters, "ii5"),
                GetParameterInteger(parameters, "ii6")
            };
            int[] j = new int[]
            {
                GetParameterInteger(parameters, "jj1"),
                GetParameterInteger(parameters, "jj2"),
                GetParameterInteger(parameters, "jj3"),
                GetParameterInteger(parameters, "jj4"),
                GetParameterInteger(parameters, "jj5"),
                GetParameterInteger(parameters, "jj6")
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
            i = SetByte(i, matrix[j][0], 0);
            i = SetByte(i, matrix[j][3], 1);
            i = SetByte(i, matrix[j][2], 2);
            i = SetByte(i, matrix[j][1], 3);
            return i;
        }

        public override string GetProviderName()
        {
            return "PowertrainSecurityAlgo2";
        }
    }
}

