using System;
using BankingSolution.Logic.Interfaces;

namespace BankingSolution.Logic.Implementation
{
    public class IdGenerator : IGenerateId
    {
        public string New()
        {
            return Guid.NewGuid().ToString();    
        }
    }
}