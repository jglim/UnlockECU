using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Similar to generic PowertrainSecurityAlgo with a fixed i and j table, specific to CRD2, CRD3, CRD3S2 family
    /// </summary>
    class PowertrainDelphiSecurityAlgo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[][] dValueMatrix = new byte[][] {
                GetParameterBytearray(parameters, "D_VALUE_0"),
                GetParameterBytearray(parameters, "D_VALUE_1"),
                GetParameterBytearray(parameters, "D_VALUE_2"),
                GetParameterBytearray(parameters, "D_VALUE_3"),
                GetParameterBytearray(parameters, "D_VALUE_4"),
                GetParameterBytearray(parameters, "D_VALUE_5"),
                GetParameterBytearray(parameters, "D_VALUE_6"),
                GetParameterBytearray(parameters, "D_VALUE_7"),
            };
            byte[][] gValueMatrix = new byte[][] {
                GetParameterBytearray(parameters, "G_VALUE_0"),
                GetParameterBytearray(parameters, "G_VALUE_1"),
                GetParameterBytearray(parameters, "G_VALUE_2"),
                GetParameterBytearray(parameters, "G_VALUE_3"),
                GetParameterBytearray(parameters, "G_VALUE_4"),
                GetParameterBytearray(parameters, "G_VALUE_5"),
                GetParameterBytearray(parameters, "G_VALUE_6"),
                GetParameterBytearray(parameters, "G_VALUE_7"),
            };

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            byte[] workingSeed = new byte[] { inSeed[3], inSeed[2], inSeed[1], inSeed[0] };

            byte y = (byte)(workingSeed[1] ^ workingSeed[0]);
            int dBit2 = GetBit((byte)(workingSeed[1] ^ workingSeed[0]), 3);
            int dBit1 = GetBit(workingSeed[2], 3);
            int dBit0 = GetBit(workingSeed[3], 6);
            uint dValue = CreateDValue(dBit2, dBit1, dBit0, dValueMatrix);

            uint seedAsInt = BytesToInt(workingSeed, Endian.Little);

            uint dXorIntermediate = seedAsInt ^ dValue;

            int gBit0 = GetBit(workingSeed[1], 3);
            int gBit1 = GetBit(y, 7);
            int gBit2 = GetBit(GetByte(dXorIntermediate, 1), 2);
            uint gValue = CreateDValue(gBit2, gBit1, gBit0, gValueMatrix);

            uint seedKey = dXorIntermediate ^ gValue;
            IntToBytes(seedKey, outKey, Endian.Big);

            return true;
        }

        private uint CreateDValue(int bit2Enabled, int bit1Enabled, int bit0Enabled, byte[][] matrix)
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

        public override string GetProviderName()
        {
            return "PowertrainDelphiSecurityAlgo";
        }
    }
}
