using System;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace SGTNEOSmartContract
{
    public static class Token
    {
        public const String TOKEN_NAME = "Safeguard Token";
        public const String TOKEN_SYMBOL = "SGT";
        public const byte TOKEN_DECIMALS = 8;
        public const ulong TOKEN_DECIMALS_FACTOR = 100000000;

        public static readonly byte[] TOKEN_OWNER = "AUZALuCrDTkRktRD5dhoboRpcZMT2idzPk".ToScriptHash(); // MainNet

        // Storage key for the current total supply
        public const String TOKEN_TOTAL_SUPPLY_KEY = "total_supply";

        // Maximum number of SGT that can ever be
        public const ulong TOKEN_MAX_SUPPLY = 113400000 * TOKEN_DECIMALS_FACTOR;

        // Maximum number of SGT to be minted in sales
        public const ulong TOKEN_MAX_CROWDSALE_SUPPLY = 75297600 * TOKEN_DECIMALS_FACTOR;

        #region Methods

        const string METHOD_UNPAUSE_TRANSFERS = "unpauseTransfers";
        const string METHOD_TRANSFERS_PAUSED = "transfersPaused";

        public static string[] Methods()
        {
            return new[] {
                METHOD_UNPAUSE_TRANSFERS,
                METHOD_TRANSFERS_PAUSED
            };
        }

        #endregion

        public static Object HandleMethod(StorageContext context, string operation, params object[] args)
        {
            if (operation.Equals(METHOD_UNPAUSE_TRANSFERS))
            {
                return ResumeTransfers(context);
            }
            if (operation.Equals(METHOD_TRANSFERS_PAUSED))
            {
                return IsTransfersPaused(context);
            }

            return false;
        }

        #region Pausable

        const string UNPAUSED_KEY = "transfers_unpaused";

        public static bool ResumeTransfers(StorageContext context)
        {
            if (!Helper.IsOwner())
            {
                // Must be owner
                return false;
            }

            Storage.Put(context, UNPAUSED_KEY, 1);
            return true;
        }

        public static bool IsTransfersPaused(StorageContext context)
        {
            return Storage.Get(context, UNPAUSED_KEY).AsBigInteger() == 0;
        }

        #endregion
    }
}
