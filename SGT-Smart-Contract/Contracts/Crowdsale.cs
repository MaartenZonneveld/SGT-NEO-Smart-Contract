using System;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;

namespace SGTNEOSmartContract
{
    public static class Crowdsale
    {
        #region Methods

        const string METHOD_PRIVATE_WHITELIST_REGISTER = "privateRegister";
        const string METHOD_PRIVATE_WHITELIST_REGISTRATION_STATUS = "privateRegistrationStatus";

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
                METHOD_PRIVATE_WHITELIST_REGISTER,
                METHOD_PRIVATE_WHITELIST_REGISTRATION_STATUS,
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

        #region Storage keys

        const string PRIVATE_WHITELISTED_KEY = "private_whitelisted";

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

        #endregion

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
            if (operation.Equals(METHOD_PRIVATE_WHITELIST_REGISTER))
            {
                if (args.Length == 2)
                {
                    return PrivateWhitelistRegister(context, (byte[])args[0], (BigInteger)args[1]);
                }
            }
            if (operation.Equals(METHOD_PRIVATE_WHITELIST_REGISTRATION_STATUS))
            {
                if (args.Length == 1)
                {
                    return IsPrivateWhitelisted(context, (byte[])args[0]);
                }
            }
            if (operation.Equals(METHOD_WHITELIST_REGISTER))
            {
                return WhitelistRegister(context, args);
            }
            if (operation.Equals(METHOD_WHITELIST_REGISTRATION_STATUS))
            {
                if (args.Length == 1)
                {
                    return IsWhitelisted(context, (byte[])args[0]);
                }
            }
            if (operation.Equals(METHOD_TOKENS_SOLD))
            {
                return GetTokensSold(context);
            }
            if (operation.Equals(METHOD_CHANGE_PERSONAL_CAP))
            {
                if (args.Length == 1)
                {
                    return ChangeCrowdsalePersonalCap(context, (BigInteger)args[0]);
                }
            }
            if (operation.Equals(METHOD_CHANGE_PRESALE_START))
            {
                if (args.Length == 1)
                {
                    return ChangePresaleStartDate(context, (BigInteger)args[0]);
                }
            }
            if (operation.Equals(METHOD_CHANGE_PRESALE_END))
            {
                if (args.Length == 1)
                {
                    return ChangePresaleEndDate(context, (BigInteger)args[0]);
                }
            }
            if (operation.Equals(METHOD_CHANGE_PRESALE_NEO_RATE))
            {
                if (args.Length == 1)
                {
                    return ChangePresaleNEORate(context, (BigInteger)args[0]);
                }
            }
            if (operation.Equals(METHOD_CHANGE_CROWDSALE_START))
            {
                if (args.Length == 1)
                {
                    return ChangeCrowdsaleStartDate(context, (BigInteger)args[0]);
                }
            }
            if (operation.Equals(METHOD_CHANGE_CROWDSALE_END))
            {
                if (args.Length == 1)
                {
                    return ChangeCrowdsaleEndDate(context, (BigInteger)args[0]);
                }
            }
            if (operation.Equals(METHOD_CHANGE_CROWDSALE_NEO_RATE))
            {
                if (args.Length == 1)
                {
                    return ChangeCrowdsaleNEORate(context, (BigInteger)args[0]);
                }
            }
            if (operation.Equals(METHOD_CONTRIBUTE))
            {
                return ContributeToSale(context);
            }
            if (operation.Equals(METHOD_AIRDROP))
            {
                if (args.Length == 2)
                {
                    return AirdropTokens(context, (byte[])args[0], (BigInteger)args[1]);
                }
            }

            return false;
        }

        #region Private Sale Whitelisting

        public static bool PrivateWhitelistRegister(StorageContext context, byte[] address, BigInteger personalCap)
        {
            if (!Helper.IsOwner())
            {
                return false;
            }

            if (!Helper.IsValidAddress(address))
            {
                return false;
            }
            
            // Personal cap is in SGT
            Storage.Put(context, PrivateWhitelistKey(address), personalCap);

            // Also whitelist for crowdsale
            Storage.Put(context, WhitelistKey(address), 1);
            OnWhitelistRegister(address);

            return true;
        }

        static bool IsPrivateWhitelisted(StorageContext context, byte[] address)
        {
            return Storage.Get(context, PrivateWhitelistKey(address)).AsBigInteger() != 0;
        }

        static string PrivateWhitelistKey(byte[] address)
        {
            return PRIVATE_WHITELISTED_KEY + address;
        }

        #endregion

        #region Presale Whitelisting

        static int WhitelistRegister(StorageContext context, params object[] args)
        {
            if (!Helper.IsOwner())
            {
                return 0;
            }

            int savedAddressesCount = 0;

            foreach (byte[] address in args)
            {
                if (Helper.IsValidAddress(address))
                {
                    Storage.Put(context, WhitelistKey(address), 1);

                    OnWhitelistRegister(address);
                    savedAddressesCount++;
                }
            }

            return savedAddressesCount;
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

        static bool ChangeCrowdsalePersonalCap(StorageContext context, BigInteger value)
        {
            // Personal cap is in SGT
            return ChangeOwnerStorageValue(context, CROWDSALE_PERSONAL_CAP, value);
        }

        public static bool ChangePresaleStartDate(StorageContext context, BigInteger value)
        {
            return ChangeOwnerStorageValue(context, PRESALE_START_KEY, value);
        }

        public static bool ChangePresaleEndDate(StorageContext context, BigInteger value)
        {
            return ChangeOwnerStorageValue(context, PRESALE_END_KEY, value);
        }

        public static bool ChangePresaleNEORate(StorageContext context, BigInteger value)
        {
            // The rate is the number of SGT you receive for 1 NEO
            return ChangeOwnerStorageValue(context, PRESALE_NEO_RATE, value);
        }

        public static bool ChangeCrowdsaleStartDate(StorageContext context, BigInteger value)
        {
            return ChangeOwnerStorageValue(context, CROWDSALE_START_KEY, value);
        }

        public static bool ChangeCrowdsaleEndDate(StorageContext context, BigInteger value)
        {
            return ChangeOwnerStorageValue(context, CROWDSALE_END_KEY, value);
        }

        public static bool ChangeCrowdsaleNEORate(StorageContext context, BigInteger value)
        {
            // The rate is the number of SGT you receive for 1 NEO
            return ChangeOwnerStorageValue(context, CROWDSALE_NEO_RATE, value);
        }

        #endregion

        #region Tokenomics

        static bool AddToTokensSold(StorageContext context, BigInteger amount)
        {
            BigInteger currentSold = Storage.Get(context, CROWDSALE_TOKEN_SOLD_KEY).AsBigInteger();

            currentSold += amount;

            Storage.Put(context, CROWDSALE_TOKEN_SOLD_KEY, currentSold);

            return true;
        }

        static BigInteger GetTokensSold(StorageContext context)
        {
            return Storage.Get(context, CROWDSALE_TOKEN_SOLD_KEY).AsBigInteger();
        }

        #endregion

        #region Minting
        
        static bool ContributeToSale(StorageContext context)
        {
            byte[] sender = GetSender();

            BigInteger contributionAmountInNEO = GetContributionAmountInNEO();
            uint currentPeriod = GetCurrentPeriod(context);

            if (!CanContributeToSale(context, contributionAmountInNEO, currentPeriod))
            {
                // This should only happen in the case that there are a lot of TX on the final
                // block before the total amount is reached. A number of TX will get through
                // the verification phase because the total amount cannot be updated during that phase.
                // Because of this, there should be a process in place to manually refund tokens
                OnRefund(sender, contributionAmountInNEO);
                // TODO: Implement actual refund functionality with storage of the refunded amount.

                return false;
            }

            // Retrieve current balance of contributor
            BigInteger currentBalance = NEP5.BalanceOf(context, sender);

            // Find current SGT:NEO swap rate
            BigInteger tokenValuePerNEO = CurrentSwapRate(context, currentPeriod);

            // Calculate the amount of SGT to be bought
            BigInteger tokenValueAmount = tokenValuePerNEO * contributionAmountInNEO;

            // Add SGT to the new total
            BigInteger newBalance = currentBalance + tokenValueAmount;

            Storage.Put(context, sender, newBalance);

            AddToTokensSold(context, tokenValueAmount);
            NEP5.AddToTotalSupply(context, tokenValueAmount);

            OnTransfer(null, sender, tokenValueAmount);

            if (IsTimeInCrowdsale(currentPeriod) || IsTimeInPresale(currentPeriod))
            {
                string key = CrowdsaleContributedKey(sender);

                BigInteger tokenValueContributed = Storage.Get(context, key).AsBigInteger();
                BigInteger newTokenValueContributed = tokenValueContributed + tokenValueAmount;

                Storage.Put(context, key, newTokenValueContributed);
            }

            if (IsTimeInPrivateSale(currentPeriod))
            {
                BigInteger personalPrivateSaleCap = Storage.Get(context, PrivateWhitelistKey(sender)).AsBigInteger();
                BigInteger newPersonalPrivateSaleCap = personalPrivateSaleCap - tokenValueAmount;
                Storage.Put(context, PrivateWhitelistKey(sender), newPersonalPrivateSaleCap);
            }

            return true;
        }

        public static bool CanContributeToSale(StorageContext context)
        {
            BigInteger contributionAmountInNEO = GetContributionAmountInNEO();
            uint currentPeriod = GetCurrentPeriod(context);
            return CanContributeToSale(context, contributionAmountInNEO, currentPeriod);
        }

        static bool CanContributeToSale(StorageContext context, BigInteger contributionAmountInNEO, uint currentPeriod)
        {
            byte[] sender = GetSender();

            if (sender.Length == 0)
            {
                return false;
            }

            if (contributionAmountInNEO <= 0)
            {
                return false;
            }

            BigInteger tokenValuePerNEO = CurrentSwapRate(context, currentPeriod);
            BigInteger tokenValueRequested = contributionAmountInNEO * tokenValuePerNEO;

            BigInteger currentlySold = GetTokensSold(context);
            BigInteger newSold = currentlySold + tokenValueRequested;

            BigInteger maxSupply = Token.TOKEN_MAX_CROWDSALE_SUPPLY;

            if (newSold > maxSupply)
            {
                // Sold out already
                return false;
            }

            if (CanContributeToPrivateSale(context, sender, tokenValueRequested, currentPeriod))
            {
                return true;
            }

            if (CanContributeToPublicSale(context, sender, tokenValueRequested, currentPeriod))
            {
                return true;
            }

            return false;
        }

        static bool CanContributeToPrivateSale(StorageContext context, byte[] sender, BigInteger tokenValueRequested, uint currentPeriod)
        {
            if (!IsTimeInPrivateSale(currentPeriod))
            {
                return false;
            }

            if (!IsPrivateWhitelisted(context, sender))
            {
                return false;
            }

            BigInteger personalPrivateSaleCap = Storage.Get(context, PrivateWhitelistKey(sender)).AsBigInteger();

            if (tokenValueRequested > personalPrivateSaleCap)
            {
                return false;
            }

            return true;
        }

        static bool CanContributeToPublicSale(StorageContext context, byte[] sender, BigInteger tokenValueRequested, uint currentPeriod)
        {
            // Check if in presale or crowdsale
            if (!IsTimeInCrowdsale(currentPeriod) && !IsTimeInPresale(currentPeriod))
            {
                return false;
            }

            if (!IsWhitelisted(context, sender))
            {
                return false;
            }

            BigInteger crowdsalePersonalCap = Storage.Get(context, CROWDSALE_PERSONAL_CAP).AsBigInteger();

            // Check if below personal cap
            if (tokenValueRequested > crowdsalePersonalCap)
            {
                return false;
            }

            string crowdsaleContributedKey = CrowdsaleContributedKey(sender);

            // Get the token value that is already contributed by the sender
            BigInteger tokenValueAlreadyContributed = Storage.Get(context, crowdsaleContributedKey).AsBigInteger();

            BigInteger newTokenValue = tokenValueAlreadyContributed + tokenValueRequested;

            // Check if new amount is still below the personal cap
            if (newTokenValue <= crowdsalePersonalCap)
            {
                return true;
            }

            return false;
        }

        static string CrowdsaleContributedKey(byte[] address)
        {
            return CROWDSALE_CONTRIBUTED_KEY + address;
        }

        // Swap rate factor = the amount of SGT you get for 1 NEO
        static BigInteger CurrentSwapRate(StorageContext context, uint currentPeriod)
        {
            if (IsTimeInPrivateSale(currentPeriod) || IsTimeInPresale(currentPeriod))
            {
                return Storage.Get(context, PRESALE_NEO_RATE).AsBigInteger();
            }

            return Storage.Get(context, CROWDSALE_NEO_RATE).AsBigInteger();
        }

        #endregion

        #region Airdropping

        static bool AirdropTokens(StorageContext context, byte[] address, BigInteger amount)
        {
            if (!Helper.IsOwner())
            {
                return false;
            }

            if (!Helper.IsValidAddress(address))
            {
                return false;
            }

            if (amount <= 0)
            {
                return false;
            }

            BigInteger currentTotalSupply = NEP5.TotalSupply(context);

            BigInteger newAmount = currentTotalSupply + amount;

            // Check if not going over max supply
            if (newAmount > Token.TOKEN_MAX_SUPPLY)
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
            TransactionOutput[] references = tx.GetReferences();

            foreach (TransactionOutput output in references)
            {
                if (output.AssetId.Equals(NEP5.NEO_ASSET_ID))
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

        // This returns the amount of NEO attached (which is multiplied by decimal factor)
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

        static bool ChangeOwnerStorageValue(StorageContext context, string key, BigInteger value)
        {
            if (!Helper.IsOwner())
            {
                return false;
            }

            Storage.Put(context, key, value);
            return true;
        }

        #endregion

        #region Periods

        const uint CURRENT_PERIOD_NO_SALE = 0;
        const uint CURRENT_PERIOD_PRIVATESALE = 1;
        const uint CURRENT_PERIOD_PRESALE = 2;
        const uint CURRENT_PERIOD_CROWDSALE = 3;

        static bool IsTimeInPrivateSale(uint currentPeriod)
        {
            return currentPeriod == CURRENT_PERIOD_PRIVATESALE;
        }

        static bool IsTimeInPresale(uint currentPeriod)
        {
            return currentPeriod == CURRENT_PERIOD_PRESALE;
        }

        static bool IsTimeInCrowdsale(uint currentPeriod)
        {
            return currentPeriod == CURRENT_PERIOD_CROWDSALE;
        }

        static uint GetCurrentPeriod(StorageContext context)
        {
            uint current = Runtime.Time;

            uint presaleStart = (uint)Storage.Get(context, PRESALE_START_KEY).AsBigInteger();
            if (current < presaleStart)
            {
                return CURRENT_PERIOD_PRIVATESALE;
            }

            uint presaleEnd = (uint)Storage.Get(context, PRESALE_END_KEY).AsBigInteger();
            if (current >= presaleStart && current <= presaleEnd)
            {
                return CURRENT_PERIOD_PRESALE;
            }

            uint crowdsaleEnd = (uint)Storage.Get(context, CROWDSALE_END_KEY).AsBigInteger();
            if (current > crowdsaleEnd)
            {
                return CURRENT_PERIOD_NO_SALE;
            }

            uint crowdsaleStart = (uint)Storage.Get(context, CROWDSALE_START_KEY).AsBigInteger();
            if (current >= crowdsaleStart)
            {
                return CURRENT_PERIOD_CROWDSALE;
            }

            return CURRENT_PERIOD_NO_SALE;
        }

        #endregion
    }
}
