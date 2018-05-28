using System;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using SGT_NEO_Smart_Contract;

namespace SGTNEOSmartContract
{
    public class SGTSmartContract : SmartContract
    {
        const string DEPLOYED_KEY = "deployed";

        public static Object Main(string operation, params object[] args)
        {
            switch (Runtime.Trigger)
            {
                // This is used in the Verification portion of the contract to determine 
                // whether a transfer of NEO involving this contract's address can proceed
                case TriggerType.Verification:

                    // check if the invoker is the owner of this contract
                    bool isOwner = Runtime.CheckWitness(Token.TOKEN_OWNER);

                    // If owner, proceed
                    if (isOwner)
                    {
                        return isOwner;
                    }

                    // Otherwise, we need to check if invoker can contribute
                    return Crowdsale.CanContributeToCrowdsale(Storage.CurrentContext);

                case TriggerType.Application:

                    foreach (var method in NEP5.Methods())
                    {
                        if (operation.Equals(method))
                        {
                            return NEP5.HandleMethod(Storage.CurrentContext, operation, args);
                        }
                    }

                    foreach (string method in Crowdsale.Methods())
                    {
                        if (operation.Equals(method))
                        {
                            return Crowdsale.HandleMethod(Storage.CurrentContext, operation, args);
                        }
                    }

                    foreach (string method in Token.Methods())
                    {
                        if (operation.Equals(method))
                        {
                            return Token.HandleMethod(Storage.CurrentContext, operation, args);
                        }
                    }

                    break;
            }

            return false;
        }

        public static bool Deploy(StorageContext context)
        {
            if (!Runtime.CheckWitness(Token.TOKEN_OWNER))
            {
                // Must be owner to deploy
                return false;
            }

            if (Storage.Get(context, DEPLOYED_KEY).Length == 0)
            {
                Storage.Put(context, DEPLOYED_KEY, 1);

                NEP5.AddToTotalSupply(context, 0);
                Crowdsale.AddToCrowdsaleTokensSold(context, 0);

                // TODO: add additional logic here

                return true;
            }

            return false;
        }
    }
}
