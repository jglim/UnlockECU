using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace UnlockECU
{
    /// <summary>
    /// Subaru SecurityAccess2018CY1 from SSM4
    /// Seed is encrypted through AES128-ECB using a variant-specific key
    /// Led and confirmed by @jnewb1 at https://github.com/jglim/UnlockECU/issues/25
    /// </summary>
    class SubaruSecurityAccess2018CY1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 16) || (outKey.Length != 16))
            {
                return false;
            }

            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.ECB;
                aes.BlockSize = 128;
                aes.KeySize = 128;
                aes.Padding = PaddingMode.None;
                aes.Key = GetParameterBytearray(parameters, "K");

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                encryptor.TransformBlock(inSeed, 0, inSeed.Length, outKey, 0);
            }

            return true;
        }
        public override string GetProviderName()
        {
            return "SubaruSecurityAccess2018CY1";
        }
    }
}