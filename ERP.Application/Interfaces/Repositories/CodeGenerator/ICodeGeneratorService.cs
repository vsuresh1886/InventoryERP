using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.CodeGenerator
{
    public  interface ICodeGeneratorService
    {
        Task<string> GenerateAsync(string moduleName);
        Task<string> GenerateSku(long domainId, long categoryId);
    }
}
