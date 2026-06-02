using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface ICollectionService
    {
        Task<List<DropdownDto>> FetchCollectionids();
        Task<GridDataResponse<CollectionGridDataDto>> FetchCollections(CollectionGridDto collection);
        Task<CollectionDto> FetchCollectionById(long collectionid);
        Task<ApiResponse<CollectionSaveDto>> CreateUpdateCollection(CollectionSaveDto CollectionDto);
        Task<List<OutstandingInvoiceCDto>> GetOutstandingbyCustomer(long customerid);
    }
}
