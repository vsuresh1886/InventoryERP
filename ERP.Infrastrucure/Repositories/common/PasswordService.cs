using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ERP.Infrastructure.Repositories.common
{
    public  class PasswordService:IPasswordService
    {
        

        public string HashPassword(string password)
        {
          
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            // WorkFactor 11 balances strong security with server computation speed (~100-200ms)
            return BCryptNet.HashPassword(password, workFactor: 11);
        }

        /// <summary>
        /// Validates an incoming plain text password against the stored hash during Login.
        /// </summary>
        /// <param name="plainPassword">The password entered into the login form.</param>
        /// <param name="hashedPassword">The secure hash string fetched from PostgreSQL.</param>
        /// <returns>True if the password matches, false otherwise.</returns>
        public  bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                // BCrypt extracts the salt automatically from the hashedPassword string to verify
                return BCryptNet.Verify(plainPassword, hashedPassword);
            }
            catch
            {
                // Handle parsing anomalies gracefully (e.g., if a legacy plain password accidentally slipped into DB)
                return false;
            }
        }


    }
}
