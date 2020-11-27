using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand

namespace UnlockECU
{
    /// <summary>
    /// Similar to RDU222, with an extra XOR
    /// </summary>
    class RVC222_MPC222_FCW246_LRR3 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            long paramA = BytesToInt(GetParameterBytearray(parameters, "A"), Endian.Big);
            long paramB = BytesToInt(GetParameterBytearray(parameters, "B"), Endian.Big);
            long paramC = BytesToInt(GetParameterBytearray(parameters, "C"), Endian.Big);

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            long inSeedAsLong = BytesToInt(inSeed, Endian.Big);

            inSeedAsLong |= paramA;
            inSeedAsLong ^= paramB;
            inSeedAsLong += paramC;
            inSeedAsLong ^= 0xFFFFFFFF;

            IntToBytes((uint)inSeedAsLong, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "RVC222_MPC222_FCW246_LRR3";
        }
    }
}
