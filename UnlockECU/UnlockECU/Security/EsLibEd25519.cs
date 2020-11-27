using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Used for modern ECUs. Ed25519PH with an empty context.
    /// </summary>
    class EsLibEd25519 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] privateKeyBytes = GetParameterBytearray(parameters, "PrivateKey");

            if ((inSeed.Length != 32) || (outKey.Length != 64))
            {
                return false;
            }

            Ed25519PrivateKeyParameters privateKey = new Ed25519PrivateKeyParameters(privateKeyBytes, 0);
            Ed25519phSigner signer = new Ed25519phSigner(new byte[] { });
            signer.Init(true, privateKey);
            signer.BlockUpdate(inSeed, 0, inSeed.Length);
            byte[] signature = signer.GenerateSignature();
            Array.ConstrainedCopy(signature, 0, outKey, 0, signature.Length);
            return true;
        }
        public override string GetProviderName()
        {
            return "EsLibEd25519";
        }
    }
}
