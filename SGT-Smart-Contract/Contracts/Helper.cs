using Neo.SmartContract.Framework.Services.Neo;
using System;

namespace SGTNEOSmartContract
{
    static class Helper
    {
        public static bool IsOwner()
        {
            return Runtime.CheckWitness(Token.TOKEN_OWNER);
        }

        public static bool IsValidAddress(byte[] address)
        {
            return address.Length == 20;
        }

        public static byte[] StorageKey(string prefix, params object[] args)
        {
            byte[] prefixArray = Neo.SmartContract.Framework.Helper.AsByteArray(prefix);
            byte[] key = Neo.SmartContract.Framework.Helper.Concat(prefixArray, (byte[])args[0]);

            for (int i = 1; i < args.Length; i++)
            {
                key = Neo.SmartContract.Framework.Helper.Concat(key, (byte[])args[i]);
            }

            return key;
        }
    }
}
