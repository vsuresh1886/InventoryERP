using ERP.Application.DTOs;
using ERP.Application.DTOs.Inventory;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities.Inventory;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class InventoryService:IInventoryService
    {
        public readonly AppDbContext _InvDbContext;
        public readonly ICodeGeneratorService _codeGeneratorservice;
        public readonly ICurrentUser _currentuser;
        public InventoryService(AppDbContext invDbContext,ICodeGeneratorService codeGeneratorService,ICurrentUser currentuser)
        {
            _InvDbContext = invDbContext;
            _codeGeneratorservice = codeGeneratorService;
            _currentuser = currentuser;
        }   

        public async Task<GridDataResponse<InventoryGridDto>> Getinventory(InvDataGridRequestDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                var query = _InvDbContext.inventoryGridViews.AsQueryable();

                

                if (!string.IsNullOrEmpty(filters.Search))
                {
                    query = query.Where(x =>
                        x.itemname.Contains(filters.Search) ||
                        x.sku.Contains(filters.Search));
                }

                if(!string.IsNullOrEmpty(filters.sku))
                {
                    query = query.Where(x => x.sku.Contains(filters.sku));
                }
                if(!string.IsNullOrEmpty(filters.itemName))
                {
                    query = query.Where(x => x.itemname.Contains(filters.itemName));
                }

                if (!string.IsNullOrEmpty(filters.category))
                {
                    query = query.Where(x => x.category == filters.category);
                }

                if (filters.lowStock == true)
                {
                    query = query.Where(x =>
                        x.maxstock != null && x.quantity < x.maxstock * 0.2m);
                }

                var total = await query.CountAsync();

                var data = await query
                            .OrderBy(x => x.itemname) // IMPORTANT: Always order before Skip
                            .Skip(offset)
                            .Take(limit).ToListAsync();
                 var res = data.Select(x => new InventoryGridDto
                            {
                                Id = x.id.ToString(),
                                Sku = x.sku,
                                part_number = x.partnumber,
                                item_name = x.itemname,

                                Tags = x.tags != null
                                    ? x.tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                                    : new List<string>(),

                                category = x.category,
                                sub_category = x.subcategory,
                                domain = x.domain,

                                quantity = x.quantity,
                                max_stock = x.maxstock,
                                unit = x.unit,
                                unit_price = x.unit_price, // extend later

                                status = x.quantity > 0 ? "Active" : "Inactive"
                            }).ToList();

                return new GridDataResponse<InventoryGridDto>
                {
                    Data = res,
                    Total = total
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine("FetchGridData error: " + ex.Message);
                throw ex;
            }
        }

        public async Task<InventoryItemDto> GetInvdtl(int id)
        {
            try
            {


                //var result = await (
                //            from i in _InvDbContext.itemmasters

                //            join v in _InvDbContext.inventoryGridViews
                //                on i.id equals v.id into viewJoin
                //            from view in viewJoin.DefaultIfEmpty()

                //            join cfg in _InvDbContext.iteminventoryconfigs
                //                on i.id equals cfg.item_id into cfgJoin
                //            from config in cfgJoin.DefaultIfEmpty()

                //            let attrs = _InvDbContext.itemattributes.Where(a => a.item_id == i.id)

                //            where i.id == id

                //            select new InventoryItemDto
                //            {
                //                sku = i.sku ?? "",
                //                item_name = i.name ?? "",
                //                description = i.description ?? "",

                //                category = i.category_id.ToString(),
                //                sub_category = i.sub_category_id.ToString(),
                //                domain = i.domain_id.ToString(),

                //                // ✅ FROM VIEW (BEST)
                //                quantity = view != null ? view.quantity.ToString() : "0",

                //                min_stock = config != null ? config.min_stock.ToString() : "0",
                //                max_stock = config != null ? config.max_stock.ToString() : "0",

                //                unit = i.unit_id.ToString(),

                //                // ✅ Also can take from view
                //                unit_price = view != null ? view.unit_price.ToString() : "0",

                //                part_number = attrs
                //                    .Where(a => a.attribute_name == "part_number")
                //                    .Select(a => a.attribute_value)
                //                    .FirstOrDefault() ?? "",

                //                warranty_months = attrs
                //                    .Where(a => a.attribute_name == "warranty_months")
                //                    .Select(a => a.attribute_value)
                //                    .FirstOrDefault() ?? "0",

                //                compatible_with = attrs
                //                    .Where(a => a.attribute_name == "compatible_with")
                //                    .Select(a => a.attribute_value)
                //                    .FirstOrDefault() ?? "",

                //                tags = attrs
                //                    .Where(a => a.attribute_name == "tags")
                //                    .Select(a => a.attribute_value)
                //                    .FirstOrDefault() != null
                //                        ? attrs
                //                            .Where(a => a.attribute_name == "tags")
                //                            .Select(a => a.attribute_value)
                //                            .FirstOrDefault()
                //                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                //                            .ToList()
                //                        : new List<string>(),

                //                status = i.is_active ? "1" : "0"
                //            }
                //        ).FirstOrDefaultAsync();


                var itemData = await (
                        from i in _InvDbContext.itemmasters

                        join v in _InvDbContext.inventoryGridViews
                            on i.id equals v.id into viewJoin
                        from view in viewJoin.DefaultIfEmpty()

                        join cfg in _InvDbContext.iteminventoryconfigs
                            on i.id equals cfg.item_id into cfgJoin
                        from config in cfgJoin.DefaultIfEmpty()

                        where i.id == id

                        select new
                        {
                            i,
                            view,
                            config
                        }
                    ).FirstOrDefaultAsync();
                var attrs = await _InvDbContext.itemattributes
                    .Where(a => a.item_id == id)
                    .ToListAsync();
                var result = itemData == null ? null : new InventoryItemDto
                {
                    id=itemData.i.id,
                    sku = itemData.i.sku ?? "",
                    item_name = itemData.i.name ?? "",
                    description = itemData.i.description ?? "",

                    category = itemData.i.category_id,
                    sub_category = itemData.i.sub_category_id,
                    domain = itemData.i.domain_id ,

                    // ✅ From VIEW
                    quantity = itemData.view?.quantity ?? 0,

                    min_stock = itemData.config?.min_stock ?? 0,
                    max_stock = itemData.config?.max_stock ?? 0,

                    unit = itemData.i.unit_id ,

                    // ✅ From VIEW (pricing strategy)
                    unit_price = itemData.view?.unit_price ?? 0,

                    supplier = 0, // TODO
                    location_bin = "", // TODO

                    part_number = itemData.view?.partnumber ?? "",

                    // ✅ Attributes
                    //part_number = attrs
                    //.FirstOrDefault(a => a.attribute_name == "part_number")?.attribute_value ?? "",

                     warranty_months = attrs.FirstOrDefault(a => a.attribute_name == "warranty_months")?.attribute_value ?? "0",

                     compatible_with = attrs
                    .FirstOrDefault(a => a.attribute_name == "compatible_with")?.attribute_value ?? "",

                     tags = attrs
                    .FirstOrDefault(a => a.attribute_name == "tags")?.attribute_value?? "",

                    status = itemData.i.is_active ? "1" : "0"
                };


                if (result != null)
            {
                return result;
            }
            else
            {
                return new InventoryItemDto { };
            }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public async Task<InventoryItemDto> CreateUpdateInv (InventoryItemDto inventoryItem)
        {

            // begin the transaction becuase here going to update nultiple tables 
            using var transaction = await _InvDbContext.Database.BeginTransactionAsync();

            try
            {


                var inv_tran = 0;
            //declare the tables we need to insert or update with linq or EF 
            Itemmaster itemmasters;
            Iteminventoryconfig itminvconfig;
            List<Itemattributes> itmattrib = new List<Itemattributes>();
            InventoryTransaction invtrans;

            if(inventoryItem.id == 0)
            {
                    inv_tran = 1; // to enable the insert in the transaction table.
                //generate the SKU based on the skugenerator (domain + categroy + serial); you have to pass the domain and catergory id to that

                string skucode;
                do
                {
                    skucode = await _codeGeneratorservice.GenerateSku(inventoryItem.domain, inventoryItem.category);
                } while (await _InvDbContext.itemmasters.AnyAsync(x => x.sku == skucode));

                inventoryItem.sku = skucode;

                    itemmasters = new Itemmaster
                    {
                        sku = skucode,
                        name = inventoryItem.item_name,
                        description = inventoryItem.description,
                        unit_id = (long)inventoryItem.unit,
                        category_id = inventoryItem.category,
                        sub_category_id = inventoryItem.sub_category,
                        domain_id = inventoryItem.domain,
                        is_active = true,
                        created_by = Convert.ToInt32(_currentuser.UserId),
                        created_at= DateTime.UtcNow
                    };

                _InvDbContext.itemmasters.Add(itemmasters);
                await _InvDbContext.SaveChangesAsync();
               

            } // UPDATE
            else
            {
                // check if the item master is not null 
                itemmasters= await _InvDbContext.itemmasters.FirstOrDefaultAsync(x=> x.id== inventoryItem.id && x.sku == inventoryItem.sku);

                if (itemmasters == null)
                    throw new Exception("Invalid Item");




                itemmasters.name = inventoryItem.item_name;
                itemmasters.description = inventoryItem.description;
                itemmasters.unit_id = (long)inventoryItem.unit;
                itemmasters.category_id = inventoryItem.category;
                itemmasters.sub_category_id = inventoryItem.sub_category;
                itemmasters.domain_id = inventoryItem.domain;
                itemmasters.is_active = true;
                itemmasters.updated_by = Convert.ToInt32(_currentuser.UserId);
                    itemmasters.updated_at = DateTime.UtcNow;
                //created_by need to add and updated by need to add 
                await _InvDbContext.SaveChangesAsync();

            }

            // Handle the config table
            itminvconfig = await _InvDbContext.iteminventoryconfigs.FirstOrDefaultAsync(x=>x.item_id == itemmasters.id);

            if (itminvconfig == null)
            {
                itminvconfig = new Iteminventoryconfig
                {
                    item_id = itemmasters.id,
                    min_stock = (decimal)inventoryItem.min_stock,
                    max_stock = (decimal)inventoryItem.max_stock,
                    // default_location_id = inventoryItem.location_bin
                };
                _InvDbContext.iteminventoryconfigs.Add(itminvconfig);
                await _InvDbContext.SaveChangesAsync();

            }
            else
            {
                itminvconfig.min_stock = (decimal)inventoryItem.min_stock;
                itminvconfig.max_stock = (decimal)inventoryItem.max_stock;
            }
            await _InvDbContext.SaveChangesAsync();


            //handle item attributes 
   
            itmattrib = await _InvDbContext.itemattributes.Where(x => x.item_id == itemmasters.id).ToListAsync();


            var newValues = new Dictionary<string, string>
                {
                    { "part_number", inventoryItem.part_number ?? "" },
                    { "tags", inventoryItem.tags != null ? string.Join(",", inventoryItem.tags) : "" }
                };
            var itemattadd = new List<Itemattributes>();

            foreach (var kvp in newValues)
            {
                var existing = itmattrib.FirstOrDefault(x => x.attribute_name == kvp.Key);
                if (existing != null)
                {
                    // 🔹 UPDATE
                    existing.attribute_value = kvp.Value;
                }
                else
                {
                    // 🔹 INSERT
                    itemattadd.Add(new Itemattributes
                    {
                        item_id = itemmasters.id,
                        attribute_name = kvp.Key,
                        attribute_value = kvp.Value
                    });
                }
            }

            if(itemattadd.Any())
            {
                _InvDbContext.itemattributes.AddRange(itemattadd);
            }
            await _InvDbContext.SaveChangesAsync();

            // final Update: Inventory transaction table - Here if the reocrd is new then it should insert record into the table other operations update is not allowed.
            
            if(inv_tran == 1)
                {
                    invtrans = new InventoryTransaction
                    {
                        item_id = itemmasters.id,
                        transaction_type = "IN",
                        transaction_date = DateTime.UtcNow,
                        quantity = inventoryItem.quantity,
                        unit_price = inventoryItem.unit_price,
                        reference_type = "OPENING",
                        warehouse_id = 1,
                        location_id = 1,
                        created_at = DateTime.UtcNow,
                        created_by = _currentuser.UserId
                    };
                    _InvDbContext.inventoryTransactions.Add(invtrans);
                    await _InvDbContext.SaveChangesAsync();
                }


            // comitt the changes
            await transaction.CommitAsync();

                return inventoryItem;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }

        }

    }
}
