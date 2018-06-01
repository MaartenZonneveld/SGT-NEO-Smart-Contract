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

        // This is the script hash of the address for the owner of the token
        // This can be found in ``neo-python`` with the walet open, use ``wallet`` command
        //public static readonly byte[] TOKEN_OWNER = "".ToScriptHash(); // MainNet
        public static readonly byte[] TOKEN_OWNER = "ATrzHaicmhRj15C3Vv6e6gLfLqhSD2PtTr".ToScriptHash(); // TestNet

        public const String TOKEN_TOTAL_SUPPLY_KEY = "total_supply";

        public const ulong TOKEN_MAX_SUPPLY = 113400000 * TOKEN_DECIMALS_FACTOR;
        public const ulong TOKEN_MAX_CROWDSALE_SUPPLY = 81000000 * TOKEN_DECIMALS_FACTOR;

        #region Methods

        const string METHOD_PAUSE_TRANSFERS = "pauseTransfers";
        const string METHOD_UNPAUSE_TRANSFERS = "unpauseTransfers";
        const string METHOD_TRANSFERS_PAUSED = "transfersPaused";

        public static string[] Methods()
        {
            return new[] {
                METHOD_PAUSE_TRANSFERS,
                METHOD_UNPAUSE_TRANSFERS,
                METHOD_TRANSFERS_PAUSED
            };
        }

        #endregion

        public static Object HandleMethod(StorageContext context, string operation, params object[] args)
        {
            if (operation.Equals(METHOD_PAUSE_TRANSFERS))
            {
                return PauseTransfers(context);
            }
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

        public static bool PauseTransfers(StorageContext context)
        {
            if (!Runtime.CheckWitness(TOKEN_OWNER))
            {
                // Must be owner
                return false;
            }
            Storage.Put(context, UNPAUSED_KEY, 0);
            return true;
        }

        public static bool ResumeTransfers(StorageContext context)
        {
            if (!Runtime.CheckWitness(TOKEN_OWNER))
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
