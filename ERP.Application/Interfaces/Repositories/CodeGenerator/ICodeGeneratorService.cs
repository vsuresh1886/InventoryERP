using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.CodeGenerator
{
    public  interface ICodeGeneratorService
    {
        Task<string> GenerateAsync(string moduleName, long? explicitCompanyId= null);
        Task<string> GenerateSku(long domainId, long categoryId, long? explicitCompanyId = null);
    }
}
