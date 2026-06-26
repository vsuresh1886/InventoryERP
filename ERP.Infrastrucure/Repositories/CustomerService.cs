using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.DirectoryServices.Protocols;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class CustomerService : ICustomerService
    {
        public readonly AppDbContext _context;
        public readonly ICodeGeneratorService _codegenerator;
        
        public CustomerService(AppDbContext context, ICodeGeneratorService codeGenerator)
        {
            _context = context;
            _codegenerator = codeGenerator;
        }

        public async Task<GridDataResponse<CustomergridDto>> FetchGridData(CDataGridRequestDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                // 🔥 Base query with joins
                var query = from em in _context.customers
                          join dm in _context.countrymasters on em.country equals dm.id
                           join dsm in _context.party_Types on em.partytype equals dsm.id
                            select new { em,dm,dsm }; //, dm, dsm

                // 🔥 Global Search
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var search = filters.Search.ToLower();

                    query = query.Where(x =>
                        (x.em.company_name).ToLower().Contains(search) || (x.em.customer_name).ToLower().Contains(search) || (x.em.customer_name).ToLower().Contains(search) ||
                        x.em.email.ToLower().Contains(search) ||
                        x.em.phone.ToLower().Contains(search) || x.dm.country_name.ToLower().Contains(search) || x.dsm.type_name.ToLower().Contains(search)
                    );
                }

                // 🔥 Name filter
                if (!string.IsNullOrWhiteSpace(filters.customerName))
                {
                    var name = filters.customerName.ToLower();

                    query = query.Where(x =>
                        (x.em.company_name).ToLower().Contains(name)
                    );
                }

                if (filters.partytype !=0 )
                {
                    query = query.Where(x => filters.partytype == 0 || x.dsm.id == filters.partytype);
                }

                // 🔥 Email filter
                if (!string.IsNullOrWhiteSpace(filters.Email))
                {
                    var email = filters.Email.ToLower();
                    query = query.Where(x => x.em.email.ToLower().Contains(email));
                }

                // 🔥 Department filter
               /* if (!string.IsNullOrWhiteSpace(filters.Department))
                {
                    query = query.Where(x => x.em.department_id.ToString() == filters.Department);
                }*/

                // 🔥 Status filter
                if (!string.IsNullOrWhiteSpace(filters.status))
                {
                    query = query.Where(x => x.em.status == filters.status);
                }

                // 🔥 Role filter
                if (filters.country != 0)
                {
                    query = query.Where(x => filters.country == 0 || x.em.country == filters.country);
                }

                // 🔥 Total count (before pagination)
                var total = await query.CountAsync();

                // 🔥 Data with pagination + projection
                var data = await query
                    .OrderBy(x => x.em.cust_pk)
                    .Skip(offset)
                    .Take(limit).ToListAsync();
                var res = data.Select((x, index) => new CustomergridDto
                {
                    Sno = offset + index + 1,
                    Id = x.em.cust_pk,
                    customerid = x.em.customer_code,
                    companyname=x.em.company_name,
                    customername = x.em.customer_name.Trim(),
                    phone = x.em.phone,
                    email = x.em.email,
                    status = x.em.status == "1"? "Active":"InActive",
                    country = x.dm.country_name,
                    partytype = x.dsm.type_name,
                    Actions = ""
                }).ToList();

                return new GridDataResponse<CustomergridDto>
                {
                    Data = res,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("FetchGridData error: " + ex.Message);
                throw;
            }
        }

        public async Task<CustomerDto> FetchCustomer(int CustomerCode)
        {
            try
            {
                var result = await _context.customers.Where(x => x.cust_pk == CustomerCode).Select(x => new CustomerDto
                {
                    customer_code = x.customer_code,
                    customer_name = x.customer_name,
                    company_name = x.company_name,
                    email = x.email,
                    phone = x.phone,
                    mobile = x.mobile ,
                    address_line1 = x.address_line1 ,
                    address_line2 = x.address_line2,
                    city = x.city,
                    state = x.state,
                    country = x.country,
                    postal_code = x.postal_code ,
                    tax_id = x.tax_id,
                    credit_limit =x.credit_limit ,
                    payment_terms = x.payment_terms,
                    website = x.website,
                    partytype = x.partytype,
                    status = x.status ,
                }).FirstOrDefaultAsync();

                if (result != null)
                {
                    return result;
                }
                else
                {
                    return new CustomerDto { };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<CustomerDto> CreateUpdateCustomer(CustomerDto customer)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                customer_master custom;

                // 🔹 CREATE
                if (string.IsNullOrEmpty(customer.customer_code))
                {
                    // 🔥 Generate unique code
                    var gencode = await _context.party_Types.Where(x => x.id == customer.partytype).Select(x => x.type_name).FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(gencode)) { } else { gencode = "Customer"; }

                    string customercode;
                    do
                    {
                        customercode = await _codegenerator.GenerateAsync(gencode);
                    }
                    while (await _context.customers.AnyAsync(x => x.customer_code == customercode));

                    customer.customer_code = customercode;

                    custom = new customer_master
                    {
                        customer_code = customer.customer_code
                    };

                    _context.customers.Add(custom);
                }
                // 🔹 UPDATE
                else
                {
                    custom = await _context.customers.FirstOrDefaultAsync(x => x.customer_code == customer.customer_code);

                    if (custom == null)
                        throw new Exception("customer not found");

                    // ❗ DO NOT create new object here
                }

                // 🔥 COMMON FIELD MAPPING
                custom.customer_name = customer.customer_name;
                custom.company_name = customer.company_name;
                custom.email = customer.email;
                custom.phone = customer.phone;
                custom.mobile = customer.mobile;
                custom.address_line1 = customer.address_line1;
                custom.address_line2 = customer.address_line2;
                custom.city = customer.city;
                custom.state = customer.state;
                custom.country = customer.country;
                custom.postal_code = customer.postal_code;
                custom.tax_id = customer.tax_id;
                custom.credit_limit = customer.credit_limit;
                custom.payment_terms = customer.payment_terms;
                custom.website = customer.website;
                custom.partytype = customer.partytype;
                custom.status = customer.status;


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return customer;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<DropdownDto>> CreateUpdateCustomerT(CustomerDto customer)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                customer_master custom;

                // 🔹 CREATE
                if (string.IsNullOrEmpty(customer.customer_code))
                {
                    // 🔥 Generate unique code
                    var gencode = await _context.party_Types.Where(x => x.id == customer.partytype).Select(x => x.type_name).FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(gencode)) { } else { gencode = "Customer"; }

                    string customercode;
                    do
                    {
                        customercode = await _codegenerator.GenerateAsync(gencode);
                    }
                    while (await _context.customers.AnyAsync(x => x.customer_code == customercode));

                    customer.customer_code = customercode;

                    custom = new customer_master
                    {
                        customer_code = customer.customer_code
                    };

                    _context.customers.Add(custom);
                }
                // 🔹 UPDATE
                else
                {
                    custom = await _context.customers.FirstOrDefaultAsync(x => x.customer_code == customer.customer_code);

                    if (custom == null)
                        throw new Exception("customer not found");

                    // ❗ DO NOT create new object here
                }

                // 🔥 COMMON FIELD MAPPING
                custom.customer_name = customer.customer_name;
                custom.company_name = customer.customer_name;
                custom.email = customer.email;
                custom.phone = customer.mobile;
                custom.mobile = customer.mobile;
                custom.address_line1 = customer.address_line1;
                custom.address_line2 = customer.address_line2;
                custom.city = customer.city;
                custom.state = customer.state;
                custom.country = customer.country;
                custom.postal_code = customer.postal_code;
                custom.tax_id = customer.tax_id;
                custom.credit_limit = customer.credit_limit;
                custom.payment_terms = customer.payment_terms;
                custom.website = customer.website;
                custom.partytype = customer.partytype;
                customer.created_at = DateTime.UtcNow;
                custom.status = customer.status;


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _context.customers.Where(x=>x.partytype==customer.partytype).Select(x => new DropdownDto
                {
                    Id = x.cust_pk,//pk as id here
                    Name = x.company_name
                }).ToListAsync();



                return result ?? null;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
