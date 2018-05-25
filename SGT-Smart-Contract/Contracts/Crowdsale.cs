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
        #region Methods

        const string METHOD_WHITELIST_REGISTER = "crowdsaleRegister";
        const string METHOD_WHITELIST_REGISTRATION_STATUS = "crowdsaleRegistrationStatus";
        const string METHOD_TOKENS_SOLD = "crowdsaleTokensSold";
        const string METHOD_CHANGE_PERSONAL_CAP = "crowdsaleChangePersonalCap";
        const string METHOD_CHANGE_PRESALE_START = "crowdsaleChangePresaleStart";
        const string METHOD_CHANGE_PRESALE_END = "crowdsaleChangePresaleEnd";
        const string METHOD_CHANGE_PRESALE_NEO_RATE = "crowdsaleChangePresaleNEORate";
        const string METHOD_CHANGE_CROWDSALE_START = "crowdsaleChangeCrowdsaleStart";
        const string METHOD_CHANGE_CROWDSALE_END = "crowdsaleChangeCrowdsaleEnd";
        const string METHOD_CHANGE_CROWDSALE_NEO_RATE = "crowdsaleChangeCrowdsaleNEORate";
        const string METHOD_CONTRIBUTE = "mintTokens";
        const string METHOD_AIRDROP = "airdropTokens";

        public static string[] Methods() {
            return new[] {
                METHOD_WHITELIST_REGISTER,
                METHOD_WHITELIST_REGISTRATION_STATUS,
                METHOD_TOKENS_SOLD,
                METHOD_CHANGE_PERSONAL_CAP,
                METHOD_CHANGE_PRESALE_START,
                METHOD_CHANGE_PRESALE_END,
                METHOD_CHANGE_PRESALE_NEO_RATE,
                METHOD_CHANGE_CROWDSALE_START,
                METHOD_CHANGE_CROWDSALE_END,
                METHOD_CHANGE_CROWDSALE_NEO_RATE,
                METHOD_CONTRIBUTE,
                METHOD_AIRDROP
            };
        }

        #endregion

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

        public static Object HandleMethod(StorageContext context, string operation, params object[] args)
        {
            if (operation.Equals(METHOD_WHITELIST_REGISTER))
            {
                return WhitelistRegister(context, args);
            }
            if (operation.Equals(METHOD_WHITELIST_REGISTRATION_STATUS))
            {
                return WhitelistRegistrationStatus(context, args);
            }
            if (operation.Equals(METHOD_TOKENS_SOLD))
            {
                return GetCrowdsaleTokensSold(context);
            }
            if (operation.Equals(METHOD_CHANGE_PERSONAL_CAP))
            {
                return ChangeCrowdsalePersonalCap(context, args);
            }
            if (operation.Equals(METHOD_CHANGE_PRESALE_START))
            {
                return ChangePresaleStartDate(context, args);
            }
            if (operation.Equals(METHOD_CHANGE_PRESALE_END))
            {
                return ChangePresaleEndDate(context, args);
            }
            if (operation.Equals(METHOD_CHANGE_PRESALE_NEO_RATE))
            {
                return ChangePresaleNEORate(context, args);
            }
            if (operation.Equals(METHOD_CHANGE_CROWDSALE_START))
            {
                return ChangeCrowdsaleStartDate(context, args);
            }
            if (operation.Equals(METHOD_CHANGE_CROWDSALE_END))
            {
                return ChangeCrowdsaleEndDate(context, args);
            }
            if (operation.Equals(METHOD_CHANGE_CROWDSALE_NEO_RATE))
            {
                return ChangeCrowdsaleNEORate(context, args);
            }
            if (operation.Equals(METHOD_CONTRIBUTE))
            {
                return CrowdsaleContribute(context);
            }
            if (operation.Equals(METHOD_AIRDROP))
            {
                return AirdropTokens(context, args);
            }

            return false;
        }

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

        static bool IsWhitelisted(StorageContext context, byte[] address)
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

            BigInteger contributionAmountInNEO = GetContributionAmountInNEO();

            if (CanContributeToCrowdsale(context))
            {
                if (InCrowdsale(context))
                {
                    string key = CrowdsaleContributedKey(sender);

                    BigInteger amountContributed = Storage.Get(context, key).AsBigInteger();
                    BigInteger newAmount = amountContributed + contributionAmountInNEO;

                    Storage.Put(context, key, newAmount);
                }
            }
            else 
            {
                // This should only happen in the case that there are a lot of TX on the final
                // block before the total amount is reached. A number of TX will get through
                // the verification phase because the total amount cannot be updated during that phase.
                // Because of this, there should be a process in place to manually refund tokens
                OnRefund(sender, contributionAmountInNEO);

                return false;
            }

            BigInteger currentBalance = NEP5.BalanceOf(context, sender);

            BigInteger currentSwapRate = CurrentSwapRate(context);

            BigInteger amount = currentSwapRate * contributionAmountInNEO;
            BigInteger newTotal = currentBalance + amount;

            Storage.Put(context, sender, newTotal);

            AddToCrowdsaleTokensSold(context, amount);

            OnTransfer(null, sender, amount);

            return true;
        }

        public static bool CanContributeToCrowdsale(StorageContext context)
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

            BigInteger contributionAmountInNEO = GetContributionAmountInNEO();

            if (contributionAmountInNEO <= 0)
            {
                return false;
            }

            BigInteger currentSwapRate = CurrentSwapRate(context);

            BigInteger amountRequested = contributionAmountInNEO * currentSwapRate;

            BigInteger currentlySoldInCrowdsale = GetCrowdsaleTokensSold(context);
            BigInteger newSoldInCrowdsale = currentlySoldInCrowdsale + amountRequested;

            if (newSoldInCrowdsale > Token.TOKEN_CROWDSALE_SUPPLY)
            {
                // Sold out already
                return false;
            }

            // Check if in presale
            if (InPresale(context))
            {
                return true;
            }

            // Check if in crowdsale
            if (InCrowdsale(context))
            {
                BigInteger crowdsalePersonalCap = Storage.Get(context, CROWDSALE_PERSONAL_CAP).AsBigInteger();

                // Check if below personal cap
                if (amountRequested > crowdsalePersonalCap)
                {
                    return false;
                }

                string crowdsaleContributedKey = CrowdsaleContributedKey(sender);

                // Get the already contributed amount
                BigInteger amountContributed = Storage.Get(context, crowdsaleContributedKey).AsBigInteger();

                BigInteger newAmount = amountContributed + amountRequested;

                // Check if new amount is still below the cap
                if (newAmount <= crowdsalePersonalCap)
                {
                    return true;
                }
            }

            return false;
        }

        static string CrowdsaleContributedKey(byte[] address)
        {
            return CROWDSALE_CONTRIBUTED_KEY + address;
        }

        static BigInteger CurrentSwapRate(StorageContext context)
        {
            BigInteger tokensPerNEO = InPresale(context) ? Storage.Get(context, PRESALE_NEO_RATE).AsBigInteger() : Storage.Get(context, CROWDSALE_NEO_RATE).AsBigInteger();

            return tokensPerNEO / (10 ^ Token.TOKEN_DECIMALS);
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

        static BigInteger GetContributionAmountInNEO()
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = tx.GetOutputs();
            BigInteger value = 0;

            foreach (TransactionOutput output in outputs)
            {
                if (output.ScriptHash == GetReceiver() && output.AssetId == NEP5.NEO_ASSET_ID)
                {
                    value += (BigInteger)output.Value;
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

        static bool InPresale(StorageContext context)
        {
            return Storage.Get(context, PRESALE_START_KEY).AsBigInteger() >= Runtime.Time && Storage.Get(context, PRESALE_END_KEY).AsBigInteger() <= Runtime.Time;
        }

        static bool InCrowdsale(StorageContext context)
        {
            return Storage.Get(context, CROWDSALE_START_KEY).AsBigInteger() >= Runtime.Time && Storage.Get(context, CROWDSALE_END_KEY).AsBigInteger() <= Runtime.Time;
        }

        #endregion
    }
}
