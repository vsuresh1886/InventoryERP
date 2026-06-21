using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.Common
{
    public interface IPasswordService
    {
        string HashPassword(string password);

        bool VerifyPassword(string plainPassword, string hashedPassword);

    }
}
