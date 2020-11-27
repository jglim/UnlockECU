using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
namespace UnlockECU
{
    /// <summary>
    /// RDU222: seed |= A, ^= B, += C
    /// </summary>
    class RDU222 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            long paramA = BytesToInt(GetParameterBytearray(parameters, "a"), Endian.Big);
            long paramB = BytesToInt(GetParameterBytearray(parameters, "b"), Endian.Big);
            long paramC = BytesToInt(GetParameterBytearray(parameters, "c"), Endian.Big);

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            long inSeedAsLong = BytesToInt(inSeed, Endian.Big);

            inSeedAsLong |= paramA;
            inSeedAsLong ^= paramB;
            inSeedAsLong += paramC;

            IntToBytes((uint)inSeedAsLong, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "RDU222";
        }
    }
}
