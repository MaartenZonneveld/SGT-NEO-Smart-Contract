using System;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using SGTNEOSmartContract;

namespace SGT_NEO_Smart_Contract
{
    public static class NEP5
    {

        #region Methods

        const string METHOD_NAME = "name";
        const string METHOD_SYMBOL = "symbol";
        const string METHOD_DECIMALS = "decimals";
        const string METHOD_TOTAL_SUPPLY = "totalSupply";
        const string METHOD_BALANCE_OF = "balanceOf";
        const string METHOD_TRANSFER = "transfer";

        public static string[] Methods() {
            return new[] {
                METHOD_NAME,
                METHOD_SYMBOL,
                METHOD_DECIMALS,
                METHOD_TOTAL_SUPPLY,
                METHOD_BALANCE_OF,
                METHOD_TRANSFER
            };
        }

        #endregion

        public delegate void MyAction<T, T1>(T p0, T1 p1);
        public delegate void MyAction<T, T1, T2>(T p0, T1 p1, T2 p2);

        [DisplayName("transfer")]
        public static event MyAction<byte[], byte[], BigInteger> Transferred;
        [DisplayName("refund")]
        public static event MyAction<byte[], BigInteger> Refund;

        public static readonly byte[] NEO_ASSET_ID = "c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b".AsByteArray();

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

            totalSupply += amount;

            Storage.Put(context, Token.TOKEN_TOTAL_SUPPLY_KEY, totalSupply);

            return true;
        }

        public static BigInteger TotalSupply(params object[] args)
        {
            return Storage.Get(Storage.CurrentContext, Token.TOKEN_TOTAL_SUPPLY_KEY).AsBigInteger();
        }

        public static BigInteger BalanceOf(params object[] args)
        {
            if (args.Length == 1)
            {
                return Storage.Get(Storage.CurrentContext, (byte[])args[0]).AsBigInteger();
            }
            return 0;
        }

        public static bool Transfer(params object[] args)
        {
            byte[] from = (byte[])args[0];
            byte[] to = (byte[])args[1];
            BigInteger amount = (BigInteger)args[2];


            if (amount <= 0)
            {
                return false;
            }
            if (to.Length != 20)
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

            BigInteger fromValue = Storage.Get(Storage.CurrentContext, from).AsBigInteger();
            if (fromValue < amount)
            {
                return false;
            }
            if (fromValue == amount)
            {
                Storage.Delete(Storage.CurrentContext, from);
            }
            else
            {
                Storage.Put(Storage.CurrentContext, from, fromValue - amount);
            }

            BigInteger toValue = Storage.Get(Storage.CurrentContext, to).AsBigInteger();
            Storage.Put(Storage.CurrentContext, to, toValue + amount);

            Transferred(from, to, amount);

            return true;
        }
    }
}
