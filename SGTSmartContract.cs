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
                return Crowdsale.CanContributeToCrowdsale(Storage.CurrentContext, true);
            }

            if (Runtime.Trigger == TriggerType.Application)
            {
                foreach (string nep5Method in NEP5.NEP5_METHODS)
                {
                    if (operation.Equals(nep5Method))
                    {
                        return NEP5.HandleNEP5(Storage.CurrentContext, operation, args);
                    }
                }

                if (operation.Equals(Crowdsale.CROWDSALE_WHITELIST_REGISTER))
                {
                    return Crowdsale.WhitelistRegister(Storage.CurrentContext, args);
                }
                if (operation.Equals(Crowdsale.CROWDSALE_WHITELIST_REGISTRATION_STATUS))
                {
                    return Crowdsale.WhitelistRegistrationStatus(Storage.CurrentContext, args);
                }
                if (operation.Equals(Crowdsale.CROWDSALE_TOKENS_SOLD))
                {
                    return Crowdsale.GetCrowdsaleTokensSold(Storage.CurrentContext);
                }
                if (operation.Equals(Crowdsale.CROWDSALE_CHANGE_PERSONAL_CAP))
                {
                    return Crowdsale.ChangeCrowdsalePersonalCap(Storage.CurrentContext, args);
                }
                if (operation.Equals(Crowdsale.CROWDSALE_GET_PERSONAL_CAP))
                {
                    return Crowdsale.GetCrowdsalePersonalCap(Storage.CurrentContext);
                }
                if (operation.Equals(Crowdsale.CROWDSALE_CONTRIBUTE))
                {
                    return Crowdsale.CrowdsaleContribute(Storage.CurrentContext);
                }
                if (operation.Equals(Crowdsale.CROWDSALE_AIRDROP))
                {
                    return Crowdsale.AirdropTokens(Storage.CurrentContext, args);
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
