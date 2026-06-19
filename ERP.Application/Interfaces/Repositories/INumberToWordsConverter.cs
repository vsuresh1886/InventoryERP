using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface INumberToWordsConverter
    {
      
        //string LanguageCode { get; } // e.g., "en", "ta", "ar"
        string Convert(decimal amount, string currencyName = "INR", string subunitName = "Paise");

    }
}
