using System;

namespace EBankApp.Helpers
{
    public static class EBankHelper
    {
        public static string GenerateAccountNumber(int length)
        {
            var random = new Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = string.Concat(s, random.Next(length).ToString());
            return s;
        }
    }
}