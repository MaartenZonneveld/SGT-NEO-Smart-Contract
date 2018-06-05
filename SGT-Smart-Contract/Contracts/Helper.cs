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
    }
}
