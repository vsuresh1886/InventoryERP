using ERP.Application.DTOs;
using ERP.Application.DTOs.Inventory;
using ERP.Application.Interfaces.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class CommonService:ICommonService
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public CommonService (AppDbContext context, IConfiguration config)
        {  _context = context; 
           _config = config;
        }

        public async Task<List<GridHeadDto>> FetchGridHeader(string formname, string gridid)
        {
            try
            {
                var GridHead =await  _context.grid_Configuration_Msts.Where(x => x.grid_id == gridid && x.form_fk == Convert.ToInt32(formname)).Select(x => new GridHeadDto
                {
                     grid_config_id = x.grid_config_id,
                            form_fk= x.form_fk,
                            grid_id=x.grid_id,
                            header_name=x.header_name,
                            display_name =x.display_name,
                            tool_tip=x.tool_tip,
                            data_type=x.data_type,
                            column_width =x.column_width,
                            text_align = x.text_align,
                            is_visible=x.is_visible,
                            is_sortable = x.is_sortable,
                            is_filterable = x.is_filterable,
                            display_order = x.display_order
                }).ToListAsync();
                
                return GridHead;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<DropdownDto>> FetchRoles()
        {
            try
            {
                var result =await  _context.designation_Masters.Select(x => new DropdownDto
                {
                    Id=x.designation_pk,
                    Name = x.designation_name
                }).ToListAsync();

                return result;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<DropdownDto>> FetchDepartment()
        {
            try
            {
                var result = await _context.department_Masters.Select(x => new DropdownDto
                {
                    Id = x.department_pk,
                    Name = x.department_name
                }).ToListAsync();

                return result;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<DropdownDto>> FetchStatus()
        {
            try
            {
                var result = new List<DropdownDto>
                                {
                                    new DropdownDto { Id = 0, Name = "InActive" },
                                    new DropdownDto { Id = 1, Name = "Active" }
                                }.ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DropdownDto>> FetchCountry()
        {
            try
            {
               

                var result = await _context.countrymasters.Select(x => new DropdownDto
                {
                    Id = x.id,//pk as id here
                    Name = x.country_name
                }).ToListAsync();

               

                return result;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DropdownDto>> FetchPartyType()
        {
            try
            {


                var result = await _context.party_Types.Select(x => new DropdownDto
                {
                    Id = x.id,//pk as id here
                    Name = x.type_name
                }).ToListAsync();


                return result;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<DropdownDto>> FetchDomain()
        {
            try
            {
                var result = await _context.domainmasters.Select(x => new DropdownDto
                {
                    Id = (int)x.id,//pk as id here
                    Name = x.name
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DropdownDto>> FetchCategory()
        {
            try
            {
                var result = await _context.categorymasters.Select(x => new DropdownDto
                {
                    Id = (int)x.id,//pk as id here
                    Name = x.name
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<DropdownDto>> FetchUnits()
        {
            try
            {
                var result = await _context.unitmasters.Select(x => new DropdownDto
                {
                    Id = (int)x.id,//pk as id here
                    Name = x.name
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DropdownDto>> FetchSupplier()
        {
            try
            {
                var result = await _context.customers.Where(x=>x.partytype == 3).Select(x => new DropdownDto
                {
                    Id = x.cust_pk,//pk as id here
                    Name = x.company_name
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DropdownDto>> FetchSubcategory()
        {
            try
            {
                var result = await _context.subcategories.Select(x => new DropdownDto
                {
                    Id =(int) x.id,//pk as id here
                    Name = x.name,
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<DropdownDto>> FetchCustomer()
        {
            try
            {
                var result = await _context.customers.Where(x => x.partytype == 1).Select(x => new DropdownDto
                {
                    Id = x.cust_pk,//pk as id here
                    Name = x.company_name
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DropdownDto>> FetchSalesperson()
        {
            try
            {
                var result = await _context.employees.Select(x => new DropdownDto
                {
                    Id = x.employee_pk,//pk as id here
                    Name = x.first_name + ' ' + x.last_name
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DropdownItemsDto>> FetchInvItems()
        {
            try
            {
                var result = await (
                                from i in _context.itemmasters

                                join v in _context.inventoryGridViews
                                    on i.id equals v.id into viewJoin
                                from view in viewJoin.DefaultIfEmpty()

                                join cfg in _context.iteminventoryconfigs
                                    on i.id equals cfg.item_id into cfgJoin
                                from config in cfgJoin.DefaultIfEmpty()
                                join attr in _context.itemattributes.Where(x=>x.attribute_name == "part_number")
                                    on i.id equals attr.item_id into attrJoin
                                from attribs in attrJoin.DefaultIfEmpty()

                                select new DropdownItemsDto
                                {
                                    id = i.id,
                                    sku = i.sku ?? "",
                                    name = i.name ?? "",

                                    // 🔥 Add this for your Autocomplete
                                    partNo = attribs.attribute_value ?? "",

                                    // Pricing
                                    price = view.unit_price,

                                    // Optional GST (use later)
                                    gstPct = config != null ? 0 : 0,
                                    availableStock = view.quantity
                                }
                            ).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
