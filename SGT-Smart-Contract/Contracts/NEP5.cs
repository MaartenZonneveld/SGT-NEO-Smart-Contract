using System;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace SGTNEOSmartContract
{
    public static class NEP5
    {
        const string ALLOWANCE_KEY = "a";
        public const string BALANCE_KEY = "b";

        #region Methods

        const string METHOD_NAME = "name";
        const string METHOD_SYMBOL = "symbol";
        const string METHOD_DECIMALS = "decimals";
        const string METHOD_TOTAL_SUPPLY = "totalSupply";
        const string METHOD_BALANCE_OF = "balanceOf";
        const string METHOD_TRANSFER = "transfer";
        const string METHOD_ALLOWANCE = "allowance";
        const string METHOD_TRANSFER_FROM = "transferFrom";
        const string METHOD_APPROVE = "approve";

        public static string[] Methods()
        {
            return new[] {
                METHOD_NAME,
                METHOD_SYMBOL,
                METHOD_DECIMALS,
                METHOD_TOTAL_SUPPLY,
                METHOD_BALANCE_OF,
                METHOD_TRANSFER,
                METHOD_ALLOWANCE,
                METHOD_TRANSFER_FROM,
                METHOD_APPROVE
            };
        }

        #endregion

        public delegate void MyAction<T, T1>(T p0, T1 p1);
        public delegate void MyAction<T, T1, T2>(T p0, T1 p1, T2 p2);

        [DisplayName("transfer")]
        public static event MyAction<byte[], byte[], BigInteger> Transferred;

        [DisplayName("approve")]
        public static event MyAction<byte[], byte[], BigInteger> Approved;

        public static readonly byte[] NEO_ASSET_ID = { 155, 124, 255, 218, 166, 116, 190, 174, 15, 147, 14, 190, 96, 133, 175, 144, 147, 229, 254, 86, 179, 74, 92, 34, 12, 205, 207, 110, 252, 51, 111, 197 };

        public static Object HandleMethod(StorageContext context, string operation, params object[] args)
        {
            if (operation.Equals(METHOD_NAME))
            {
                return Name();
            }
            if (operation.Equals(METHOD_SYMBOL))
            {
                return Symbol();
            }
            if (operation.Equals(METHOD_DECIMALS))
            {
                return Decimals();
            }
            if (operation.Equals(METHOD_TOTAL_SUPPLY))
            {
                return TotalSupply(context);
            }
            if (operation.Equals(METHOD_BALANCE_OF))
            {
                if (args.Length == 1)
                {
                    return BalanceOf(context, (byte[])args[0]);
                }
            }
            if (operation.Equals(METHOD_TRANSFER))
            {
                if (args.Length == 3)
                {
                    return Transfer(context, (byte[])args[0], (byte[])args[1], (BigInteger)args[2]);
                }
            }
            if (operation.Equals(METHOD_TRANSFER_FROM))
            {
                if (args.Length == 4)
                {
                    return TransferFrom(context, (byte[])args[0], (byte[])args[1], (byte[])args[2], (BigInteger)args[3]);
                }
            }
            if (operation.Equals(METHOD_APPROVE))
            {
                if (args.Length == 3)
                {
                    return Approve(context, (byte[])args[0], (byte[])args[1], (BigInteger)args[2]);
                }
            }
            if (operation.Equals(METHOD_ALLOWANCE))
            {
                if (args.Length == 2)
                {
                    return Allowance(context, (byte[])args[0], (byte[])args[1]);
                }
            }

            return false;
        }

        public static string Name(params object[] args)
        {
            return Token.TOKEN_NAME;
        }

        public static string Symbol(params object[] args)
        {
            return Token.TOKEN_SYMBOL;
        }

        public static byte Decimals(params object[] args)
        {
            return Token.TOKEN_DECIMALS;
        }

        public static bool AddToTotalSupply(StorageContext context, BigInteger amount)
        {
            BigInteger totalSupply = Storage.Get(context, Token.TOKEN_TOTAL_SUPPLY_KEY).AsBigInteger();

            totalSupply = totalSupply + amount;

            Storage.Put(context, Token.TOKEN_TOTAL_SUPPLY_KEY, totalSupply);

            return true;
        }

        public static BigInteger TotalSupply(StorageContext context)
        {
            return Storage.Get(context, Token.TOKEN_TOTAL_SUPPLY_KEY).AsBigInteger();
        }

        public static BigInteger BalanceOf(StorageContext context, byte[] address)
        {
            return Storage.Get(context, Helper.StorageKey(BALANCE_KEY, address)).AsBigInteger();
        }

        public static bool Transfer(StorageContext context, byte[] from, byte[] to, BigInteger amount)
        {
            if (amount <= 0)
            {
                return false;
            }
            if (!Helper.IsValidAddress(to))
            {
                return false;
            }
            if (!Runtime.CheckWitness(from))
            {
                return false;
            }
            if (from == to)
            {
                return true;
            }

            // Don't transfer when paused
            if (Token.IsTransfersPaused(context))
            {
                return false;
            }

            byte[] fromKey = Helper.StorageKey(BALANCE_KEY, from);

            BigInteger fromValue = Storage.Get(context, fromKey).AsBigInteger();
            if (fromValue < amount)
            {
                return false;
            }
            if (fromValue == amount)
            {
                Storage.Delete(context, fromKey);
            }
            else
            {
                Storage.Put(context, fromKey, fromValue - amount);
            }

            byte[] toKey = Helper.StorageKey(BALANCE_KEY, to);

            BigInteger toValue = Storage.Get(context, toKey).AsBigInteger();
            Storage.Put(context, toKey, toValue + amount);

            Transferred(from, to, amount);

            return true;
        }

        public static bool TransferFrom(StorageContext context, byte[] originator, byte[] from, byte[] to, BigInteger amount)
        {
            if (amount <= 0)
            {
                return false;
            }
            if (!Helper.IsValidAddress(to))
            {
                return false;
            }
            if (!Runtime.CheckWitness(originator))
            {
                return false;
            }
            if (from == to)
            {
                return true;
            }

            // Don't transfer when paused
            if (Token.IsTransfersPaused(context))
            {
                return false;
            }

            byte[] allowanceKey = Helper.StorageKey(ALLOWANCE_KEY, from, originator);

            BigInteger allowanceValue = Storage.Get(context, allowanceKey).AsBigInteger();

            if (allowanceValue < amount)
            {
                return false;
            }

            byte[] fromKey = Helper.StorageKey(BALANCE_KEY, from);
            BigInteger fromValue = Storage.Get(context, fromKey).AsBigInteger();

            if (fromValue < amount)
            {
                return false;
            }
            if (fromValue == amount)
            {
                Storage.Delete(context, fromKey);
            }
            else
            {
                Storage.Put(context, fromKey, fromValue - amount);
            }

            byte[] toKey = Helper.StorageKey(BALANCE_KEY, to);

            BigInteger toValue = Storage.Get(context, toKey).AsBigInteger();
            Storage.Put(context, toKey, toValue + amount);

            if (allowanceValue == amount)
            {
                Storage.Delete(context, allowanceKey);
            }
            else
            {
                Storage.Put(context, allowanceKey, allowanceValue - amount);
            }

            Transferred(from, to, amount);

            return true;
        }

        public static bool Approve(StorageContext context, byte[] owner, byte[] spender, BigInteger amount)
        {
            if (!Helper.IsValidAddress(spender))
            {
                return false;
            }

            if (amount <= 0)
            {
                return false;
            }

            if (!Runtime.CheckWitness(owner))
            {
                return false;
            }

            Storage.Put(context, Helper.StorageKey(ALLOWANCE_KEY, owner, spender), amount);

            Approved(owner, spender, amount);

            return true;
        }

        public static BigInteger Allowance(StorageContext context, byte[] owner, byte[] spender)
        {
            if (!Helper.IsValidAddress(owner))
            {
                return 0;
            }

            if (!Helper.IsValidAddress(spender))
            {
                return 0;
            }

            return Storage.Get(context, Helper.StorageKey(ALLOWANCE_KEY, owner, spender)).AsBigInteger();
        }
    }
}
