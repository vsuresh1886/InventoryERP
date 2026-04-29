using ERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface ICommonService
    {
        public Task<List<GridHeadDto>> FetchGridHeader(string formname, string gridid);
        public Task<List<DropdownDto>> FetchRoles();
        public Task<List<DropdownDto>> FetchDepartment();
        public Task<List<DropdownDto>> FetchStatus();
        public Task<List<DropdownDto>> FetchCountry();
        public Task<List<DropdownDto>> FetchPartyType();
        public Task<List<DropdownDto>> FetchDomain();
        public Task<List<DropdownDto>> FetchCategory();
        public Task<List<DropdownDto>> FetchUnits();
        public Task<List<DropdownDto>> FetchSupplier();
        public Task<List<DropdownDto>> FetchSubcategory();
        public Task<List<DropdownDto>> FetchCustomer();
        public Task<List<DropdownDto>> FetchSalesperson();
        public Task<List<DropdownItemsDto>> FetchInvItems();


    }
}
