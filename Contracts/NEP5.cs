using System;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using SGTNEOSmartContract;

namespace SGT_NEO_Smart_Contract
{
    public static class NEP5
    {
        public delegate void MyAction<T, T1>(T p0, T1 p1);
        public delegate void MyAction<T, T1, T2>(T p0, T1 p1, T2 p2);

        [DisplayName("transfer")]
        public static event MyAction<byte[], byte[], BigInteger> Transferred;
        [DisplayName("refund")]
        public static event MyAction<byte[], BigInteger> Refund;

        const string NEP5_NAME = "name";
        const string NEP5_SYMBOL = "symbol";
        const string NEP5_DECIMALS = "decimals";
        const string NEP5_TOTAL_SUPPLY = "totalSupply";
        const string NEP5_BALANCE_OF = "balanceOf";
        const string NEP5_TRANSFER = "transfer";

        public static readonly string[] NEP5_METHODS = {
            NEP5_NAME,
            NEP5_SYMBOL,
            NEP5_DECIMALS,
            NEP5_TOTAL_SUPPLY,
            NEP5_BALANCE_OF,
            NEP5_TRANSFER
        };

        public static readonly byte[] NEO_ASSET_ID = "c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b".AsByteArray();

        public static Object HandleNEP5(StorageContext context, string operation, params object[] args)
        {
            if (operation.Equals(NEP5_NAME))
            {
                return Name();
            }
            if (operation.Equals(NEP5_SYMBOL))
            {
                return Symbol();
            }
            if (operation.Equals(NEP5_DECIMALS))
            {
                return Decimals();
            }
            if (operation.Equals(NEP5_TOTAL_SUPPLY))
            {
                return TotalSupply(context);
            }
            if (operation.Equals(NEP5_BALANCE_OF))
            {
                if (args.Length == 1)
                {
                    return BalanceOf(context, (byte[])args[0]);
                }
            }
            if (operation.Equals(NEP5_TRANSFER))
            {
                if (args.Length == 3)
                {
                    return Transfer(context, (byte[])args[0], (byte[])args[1], (BigInteger)args[2]);
                }
            }

            return false;
        }

        public static string Name()
        {
            return Token.TOKEN_NAME;
        }

        public static string Symbol()
        {
            return Token.TOKEN_SYMBOL;
        }

        public static byte Decimals()
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

        public static BigInteger TotalSupply(StorageContext context)
        {
            return Storage.Get(context, Token.TOKEN_TOTAL_SUPPLY_KEY).AsBigInteger();
        }

        public static BigInteger BalanceOf(StorageContext context, byte[] address)
        {
            return Storage.Get(context, address).AsBigInteger();
        }

        public static bool Transfer(StorageContext context, byte[] from, byte[] to, BigInteger amount)
        {
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

            BigInteger fromValue = Storage.Get(context, from).AsBigInteger();
            if (fromValue < amount)
            {
                return false;
            }
            if (fromValue == amount)
            {
                Storage.Delete(context, from);
            }
            else
            {
                Storage.Put(context, from, fromValue - amount);
            }

            BigInteger toValue = Storage.Get(context, to).AsBigInteger();
            Storage.Put(Storage.CurrentContext, to, toValue + amount);

            Transferred(from, to, amount);

            return true;
        }
    }
}
