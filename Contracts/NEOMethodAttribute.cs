using System;
namespace SGTNEOSmartContract.Contracts
{
    public class NEOMethodAttribute : Attribute
    {
        public string Method { get; set; }
    }
}
