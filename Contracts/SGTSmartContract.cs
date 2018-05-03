using System;
using System.Reflection;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using SGT_NEO_Smart_Contract;
using SGTNEOSmartContract.Contracts;

namespace SGTNEOSmartContract
{
    public class SGTSmartContract : SmartContract
    {
        const string DEPLOYED_KEY = "deployed";

        public static Object Main(string operation, params object[] args)
        {

            // This is used in the Verification portion of the contract to determine 
            // whether a transfer of NEO involving this contract's address can proceed
            if (Runtime.Trigger == TriggerType.Verification)
            {
                // check if the invoker is the owner of this contract
                bool isOwner = Runtime.CheckWitness(Token.TOKEN_OWNER);

                // If owner, proceed
                if (isOwner)
                {
                    return isOwner;
                }

                // Otherwise, we need to check if invoker can contribute
                return Crowdsale.CanContributeToCrowdsale(Storage.CurrentContext);
            }

            if (Runtime.Trigger == TriggerType.Application)
            {
                Assembly[] assemblies = {
                    typeof(NEP5).Assembly,
                    typeof(Crowdsale).Assembly,
                };

                foreach (Assembly assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        // Check each method for the attribute.
                        foreach (var method in type.GetRuntimeMethods())
                        {
                            // Test for presence of the attribute
                            var attribute = method.GetCustomAttribute<NEOMethodAttribute>();

                            if (attribute == null)
                            {
                                continue;
                            }

                            if (attribute.Method.Equals(operation))
                            {
                                return method.Invoke(null, args);
                            }
                        }
                    }
                }
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
