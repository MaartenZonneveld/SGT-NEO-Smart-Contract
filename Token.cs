using System;
using Neo.SmartContract.Framework;

namespace SGTNEOSmartContract
{
    public static class Token
    {
        public const String TOKEN_NAME = "Safeguard Token";
        public const String TOKEN_SYMBOL = "SGT";
        public const byte TOKEN_DECIMALS = 8;

        // This is the script hash of the address for the owner of the token
        // This can be found in ``neo-python`` with the walet open, use ``wallet`` command
        //public static readonly byte[] TOKEN_OWNER = "".ToScriptHash(); // MainNet
        public static readonly byte[] TOKEN_OWNER = "ATrzHaicmhRj15C3Vv6e6gLfLqhSD2PtTr".ToScriptHash(); // TestNet

        public const String TOKEN_TOTAL_SUPPLY_KEY = "total_supply";

        public const ulong TOKEN_TOTAL_SUPPLY = 113400000 * (10 ^ TOKEN_DECIMALS);
        public const ulong TOKEN_CROWDSALE_SUPPLY = 81000000 * (10 ^ TOKEN_DECIMALS);
    }
}
