using ERP.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Humanizer;

namespace ERP.Infrastructure.Repositories.NumberstoWords
{
    public class EnglishNumberToWords
    {
        //public string LanguageCode => "en";

        public string Convert(decimal amount, string currencyName = "Dollars", string subunitName = "Cents")
        {
            long wholePart = (long)Math.Floor(amount);
            int decimalPart = (int)Math.Round((amount - wholePart) * 100);

            string wholeWords = wholePart.ToWords().Transform(To.TitleCase);
            string result = $"{wholeWords} {currencyName}";

            if (decimalPart > 0)
            {
                string decimalWords = decimalPart.ToWords().Transform(To.TitleCase);
                result += $" and {decimalWords} {subunitName}";
            }

            return result + " Only";
        }



    }
}
