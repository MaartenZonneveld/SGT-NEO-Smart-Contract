using System;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using SGT_NEO_Smart_Contract;

namespace SGTNEOSmartContract
{
    public static class Crowdsale
    {
        public const string CROWDSALE_WHITELIST_REGISTER = "crowdsaleRegister";
        public const string CROWDSALE_WHITELIST_REGISTRATION_STATUS = "crowdsaleRegistrationStatus";
        public const string CROWDSALE_TOKENS_SOLD = "crowdsaleTokensSold";
        public const string CROWDSALE_CHANGE_PERSONAL_CAP = "crowdsaleChangePersonalCap";
        public const string CROWDSALE_CHANGE_PRESALE_START = "crowdsaleChangePresaleStart";
        public const string CROWDSALE_CHANGE_PRESALE_END = "crowdsaleChangePresaleEnd";
        public const string CROWDSALE_CHANGE_PRESALE_NEO_RATE = "crowdsaleChangePresaleNEORate";
        public const string CROWDSALE_CHANGE_CROWDSALE_START = "crowdsaleChangeCrowdsaleStart";
        public const string CROWDSALE_CHANGE_CROWDSALE_END = "crowdsaleChangeCrowdsaleEnd";
        public const string CROWDSALE_CHANGE_CROWDSALE_NEO_RATE = "crowdsaleChangeCrowdsaleNEORate";
        public const string CROWDSALE_CONTRIBUTE = "mintTokens";
        public const string CROWDSALE_AIRDROP = "airdropTokens";

        const string WHITELISTED_KEY = "whitelisted";
        const string CROWDSALE_CONTRIBUTED_KEY = "crowdsale_contributed";

        const string CROWDSALE_PERSONAL_CAP = "crowdsale_personal_cap";
        const string CROWDSALE_TOKEN_SOLD_KEY = "tokens_sold_in_crowdsale";

        const string PRESALE_START_KEY = "presale_start";
        const string PRESALE_END_KEY = "presale_end";
        const string PRESALE_NEO_RATE = "presale_neo_rate";

        const string CROWDSALE_START_KEY = "crowdsale_start";
        const string CROWDSALE_END_KEY = "crowdsale_end";
        const string CROWDSALE_NEO_RATE = "crowdsale_neo_rate";

        public delegate void NEOEvent<T>(T p0);
        public delegate void NEOEvent<T, T1>(T p0, T1 p1);
        public delegate void NEOEvent<T, T1, T2>(T p0, T1 p1, T2 p2);

        [DisplayName("whitelistRegister")]
        public static event NEOEvent<byte[]> OnWhitelistRegister;
        [DisplayName("transfer")]
        public static event NEOEvent<byte[], byte[], BigInteger> OnTransfer;
        [DisplayName("refund")]
        public static event NEOEvent<byte[], BigInteger> OnRefund;

        #region Whitelisting

        public static int WhitelistRegister(StorageContext context, params object[] args)
        {
            int savedAddressesCount = 0;

            if (Runtime.CheckWitness(Token.TOKEN_OWNER))
            {
                foreach (byte[] address in args)
                {
                    if (address.Length == 20)
                    {
                        Storage.Put(context, WhitelistKey(address), 1);

                        OnWhitelistRegister(address);
                        savedAddressesCount++;
                    }
                }
            }

            return savedAddressesCount;
        }

        public static bool WhitelistRegistrationStatus(StorageContext context, params object[] args)
        {
            if (args.Length > 0)
            {
                return IsWhitelisted(context, (byte[])args[0]);
            }

            return false;
        }

        public static bool IsWhitelisted(StorageContext context, byte[] address)
        {
            return Storage.Get(context, WhitelistKey(address)).AsBigInteger() == 1;
        }

        static string WhitelistKey(byte[] address)
        {
            return WHITELISTED_KEY + address;
        }

        #endregion

        #region Caps, dates & rates

        public static bool ChangeCrowdsalePersonalCap(StorageContext context, params object[] args)
        {
            if (!Runtime.CheckWitness(Token.TOKEN_OWNER))
            {
                return false;
            }

            if (args.Length != 1)
            {
                return false;
            }

            Storage.Put(context, CROWDSALE_PERSONAL_CAP, (BigInteger)args[0]);
            return true;
        }

        public static bool ChangePresaleStartDate(StorageContext context, params object[] args)
        {
            return ChangeKey(context, PRESALE_START_KEY, args);
        }

        public static bool ChangePresaleEndDate(StorageContext context, params object[] args)
        {
            return ChangeKey(context, PRESALE_END_KEY, args);
        }

        public static bool ChangePresaleNEORate(StorageContext context, params object[] args)
        {
            return ChangeKey(context, PRESALE_NEO_RATE, args);
        }

        public static bool ChangeCrowdsaleStartDate(StorageContext context, params object[] args)
        {
            return ChangeKey(context, CROWDSALE_START_KEY, args);
        }

        public static bool ChangeCrowdsaleEndDate(StorageContext context, params object[] args)
        {
            return ChangeKey(context, CROWDSALE_END_KEY, args);
        }

        public static bool ChangeCrowdsaleNEORate(StorageContext context, params object[] args)
        {
            return ChangeKey(context, CROWDSALE_NEO_RATE, args);
        }

        #endregion

        #region Tokenomics

        public static bool AddToCrowdsaleTokensSold(StorageContext context, BigInteger amount)
        {
            BigInteger currentSold = Storage.Get(context, CROWDSALE_TOKEN_SOLD_KEY).AsBigInteger();

            currentSold += amount;

            Storage.Put(context, CROWDSALE_TOKEN_SOLD_KEY, currentSold);

            return true;
        }

        public static BigInteger GetCrowdsaleTokensSold(StorageContext context)
        {
            return Storage.Get(context, CROWDSALE_TOKEN_SOLD_KEY).AsBigInteger();
        }

        #endregion

        #region Minting

        public static bool CrowdsaleContribute(StorageContext context)
        {
            byte[] sender = GetSender();

            BigInteger contributionAmount = GetContributionAmount();

            if (!CanContributeToCrowdsale(context, false))
            {
                // This should only happen in the case that there are a lot of TX on the final
                // block before the total amount is reached.  An amount of TX will get through
                // the verification phase because the total amount cannot be updated during that phase.
                // Because of this, there should be a process in place to manually refund tokens
                OnRefund(sender, contributionAmount);

                return false;
            }

            BigInteger currentBalance = NEP5.BalanceOf(context, sender);

            BigInteger currentSwapRate = CurrentSwapRate();

            BigInteger amount = currentSwapRate * GetContributionAmount();

            BigInteger newTotal = currentBalance + amount;
            Storage.Put(context, sender, newTotal);

            AddToCrowdsaleTokensSold(context, amount);

            OnTransfer(null, sender, amount);

            return true;
        }

        public static bool CanContributeToCrowdsale(StorageContext context, bool verifyOnly)
        {
            byte[] sender = GetSender();

            if (sender.Length == 0)
            {
                return false;
            }

            if (!IsWhitelisted(context, sender))
            {
                return false;
            }

            ulong contributionAmount = GetContributionAmount();

            if (contributionAmount <= 0)
            {
                return false;
            }

            return CalculateCanContributeToCrowdsale(context, contributionAmount, sender, verifyOnly);
        }

        static bool CalculateCanContributeToCrowdsale(StorageContext context, ulong amount, byte[] address, bool verifyOnly)
        {
            BigInteger currentlySoldInCrowdsale = GetCrowdsaleTokensSold(context);
            BigInteger newSoldInCrowdsale = currentlySoldInCrowdsale + amount;

            if (newSoldInCrowdsale > Token.TOKEN_CROWDSALE_SUPPLY)
            {
                // Sold out already
                return false;
            }

            // TODO: Date checks

            BigInteger crowdsalePersonalCap = Storage.Get(context, CROWDSALE_PERSONAL_CAP).AsBigInteger();

            // Check if below personal cap
            if (amount <= crowdsalePersonalCap)
            {
                string key = CrowdsaleContributedKey(address);

                // Check if they have already contributed and how much
                BigInteger amountContributed = Storage.Get(context, key).AsBigInteger();

                // if not, save the amount
                if (amountContributed <= 0)
                {
                    // Only save when not verifying
                    if (!verifyOnly)
                    {
                        Storage.Put(context, key, amount);
                    }
                    return true;
                }

                // If so, check if still below cap
                BigInteger newAmount = amountContributed + amount;

                if (newAmount <= crowdsalePersonalCap)
                {
                    // Only save when not verifying
                    if (!verifyOnly)
                    {
                        Storage.Put(context, key, newAmount);
                    }
                    return true;
                }
            }

            return false;
        }

        static string CrowdsaleContributedKey(byte[] address)
        {
            return CROWDSALE_CONTRIBUTED_KEY + address;
        }

        private static ulong CurrentSwapRate()
        {
            //const int ico_duration = ico_end_time - ico_start_time;
            //uint now = Runtime.Time;
            //int time = (int)now - ico_start_time;

            //if (time < 0)
            //{
            //    return 0;
            //}
            //else if (time < ico_duration)
            //{
            //    return basic_rate;
            //}
            //else
            //{
            //    return 0;
            //}
            return 0;
        }

        #endregion

        #region Airdropping

        public static bool AirdropTokens(StorageContext context, params object[] args)
        {
            if (!Runtime.CheckWitness(Token.TOKEN_OWNER))
            {
                return false;
            }

            if (args.Length != 2)
            {
                return false;
            }

            byte[] address = (byte[])args[0];

            // Check whitelist status
            if (!IsWhitelisted(context, address))
            {
                return false;
            }

            BigInteger amount = (BigInteger)args[1];

            BigInteger currentTotalSupply = NEP5.TotalSupply(context);

            BigInteger newAmount = currentTotalSupply + amount;

            // Check if not going over total supply
            if (newAmount > Token.TOKEN_TOTAL_SUPPLY)
            {
                return false;
            }

            BigInteger currentBalance = NEP5.BalanceOf(context, address);

            BigInteger newTotal = currentBalance + amount;

            // Update balance
            Storage.Put(context, address, newTotal);

            // Update total supply
            NEP5.AddToTotalSupply(context, amount);

            OnTransfer(null, address, amount);

            return true;
        }

        #endregion

        #region Helper functions

        static byte[] GetSender()
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] reference = tx.GetReferences();

            foreach (TransactionOutput output in reference)
            {
                if (output.AssetId == NEP5.NEO_ASSET_ID)
                {
                    return output.ScriptHash;
                }
            }
            return new byte[] { };
        }

        static byte[] GetReceiver()
        {
            return ExecutionEngine.ExecutingScriptHash;
        }

        static ulong GetContributionAmount()
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = tx.GetOutputs();
            ulong value = 0;

            foreach (TransactionOutput output in outputs)
            {
                if (output.ScriptHash == GetReceiver() && output.AssetId == NEP5.NEO_ASSET_ID)
                {
                    value += (ulong)output.Value;
                }
            }
            return value;
        }

        static bool ChangeKey(StorageContext context, string key, params object[] args)
        {
            if (!Runtime.CheckWitness(Token.TOKEN_OWNER))
            {
                return false;
            }

            if (args.Length != 1)
            {
                return false;
            }

            Storage.Put(context, key, (BigInteger)args[0]);
            return true;
        }

        #endregion
    }
}
