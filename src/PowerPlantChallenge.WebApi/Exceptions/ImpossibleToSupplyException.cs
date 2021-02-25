using System;

namespace PowerPlantChallenge.WebApi.Exceptions
{
    public class ImpossibleToSupplyException : Exception
    {
        public ImpossibleToSupplyException(decimal load, string reason) :
            base($"Impossible to supply the requested ({load}) load \n Reason: {reason}")
        {
        }
    }
}