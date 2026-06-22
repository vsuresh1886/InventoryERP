using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class UserService:IUserService
    {
        private readonly AppDbContext _context;
        private readonly ICodeGeneratorService _codegenerator;
        
        public UserService(AppDbContext context, ICodeGeneratorService codeGenerator)
        {
            _context = context;
            _codegenerator = codeGenerator;
        }

        public async Task<GridDataResponse<EmployeeDto>> FetchGridData(UDataGridRequestDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                // 🔥 Base query with joins
                var query = from em in _context.employees
                            join dm in _context.department_Masters on em.department_id equals dm.department_pk
                            join dsm in _context.designation_Masters on em.designation_id equals dsm.designation_pk
                            select new { em, dm, dsm };

                // 🔥 Global Search
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var search = filters.Search.ToLower();

                    query = query.Where(x =>
                        (x.em.first_name + " " + (x.em.last_name ?? "")).ToLower().Contains(search) ||
                        x.em.email.ToLower().Contains(search) ||
                        x.em.phone.ToLower().Contains(search)
                    );
                }

                // 🔥 Name filter
                if (!string.IsNullOrWhiteSpace(filters.Name))
                {
                    var name = filters.Name.ToLower();

                    query = query.Where(x =>
                        (x.em.first_name + " " + (x.em.last_name ?? "")).ToLower().Contains(name)
                    );
                }

                // 🔥 Email filter
                if (!string.IsNullOrWhiteSpace(filters.Email))
                {
                    var email = filters.Email.ToLower();
                    query = query.Where(x => x.em.email.ToLower().Contains(email));
                }

                // 🔥 Department filter
                if (!string.IsNullOrWhiteSpace(filters.Department))
                {
                    query = query.Where(x => x.em.department_id.ToString() == filters.Department);
                }

                // 🔥 Status filter
                if (!string.IsNullOrWhiteSpace(filters.Status))
                {
                    query = query.Where(x => x.em.status == filters.Status);
                }

                // 🔥 Role filter
                if (!string.IsNullOrWhiteSpace(filters.Role))
                {
                    query = query.Where(x => x.em.designation_id.ToString() == filters.Role);
                }

                // 🔥 Total count (before pagination)
                var total = await query.CountAsync();

                // 🔥 Data with pagination + projection
                var data = await query
                    .OrderBy(x => x.em.employee_pk)
                    .Skip(offset)
                    .Take(limit).ToListAsync();
               var res =data.Select((x, index) => new EmployeeDto
                    {
                        Sno = offset + index + 1,
                        Id = x.em.employee_pk,
                        Employee_Code = x.em.employee_id,
                        Fullname = (x.em.first_name + " " + (x.em.last_name ?? "")).Trim(),
                        Email = x.em.email,
                        Designation_Id = x.em.designation_id,
                        Role = x.dsm.designation_name,
                        Department_Id = x.em.department_id,
                        Department = x.dm.department_name,
                        Status = x.em.status == "1"? "Active":"InActive",
                        Phone = x.em.phone,
                        Country = x.em.country,
                        Actions = ""
                    }).ToList();

                return new GridDataResponse<EmployeeDto>
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
        public async Task<EmployeeDetailDto> FetchUser(long employeeCode)
        {
            
            try
            {
               
               var  query = await _context.employees.Where(x => x.employee_pk == employeeCode ).Select(x => new EmployeeDetailDto
                {
                    Id = x.employee_pk,
                    Employee_Code = x.employee_id,
                    Firstname = x.first_name,
                    Lastname = x.last_name,
                    gender = x.gender,
                    dob = x.dob,
                    joining_date = x.joining_date,
                    department_id = x.department_id,
                    designation_id = x.designation_id,
                    email = x.email,
                    phone = x.phone,
                    address = x.address,
                    city = x.city,
                    state = x.state,
                    country = x.country,
                    status = x.status,
                    
                }).FirstOrDefaultAsync();
                if (query != null)
                {
                    return query;
                }
                else { return null; }
              
            }
            catch(Exception ex)
            {
               throw ex;
            }
            
             
        }


        public async Task<UserDetDto> FetchUserDet(long userId)
        {
            try
            {

                var query = await _context.employees.Where(x => x.employee_pk == userId).Select(x => new UserDetDto
                {
                    id = x.employee_pk,
                    name = x.first_name + " " + x.last_name,
                   
                    designation = _context.designation_Masters.Where(y=>y.designation_pk == x.designation_id).Select(z=>z.designation_name).FirstOrDefault(),
                    photoUrl = "  ",
                    companyName = _context.companies.Where(y=>y.id == x.company_id).Select(z=>z.company_name).FirstOrDefault(),

                }).FirstOrDefaultAsync();
                if (query != null)
                {
                    return query;
                }
                else { return null; }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<EmployeeSaveDto> CreateUpdateUser(EmployeeSaveDto employee)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                employee_master employ;

                // 🔹 CREATE
                if (string.IsNullOrEmpty(employee.Employee_Code))
                {
                    // 🔥 Generate unique code
                    string employeeCode;
                    do
                    {
                        employeeCode = await _codegenerator.GenerateAsync("Employee");
                    }
                    while (await _context.employees.AnyAsync(x => x.employee_id == employeeCode));

                    employee.Employee_Code = employeeCode;

                    employ = new employee_master
                    {
                        employee_id = employee.Employee_Code
                    };

                    _context.employees.Add(employ);
                }
                // 🔹 UPDATE
                else
                {
                    employ = await _context.employees.FirstOrDefaultAsync(x => x.employee_id == employee.Employee_Code);

                    if (employ == null)
                        throw new Exception("Employee not found");

                    // ❗ DO NOT create new object here
                }

                // 🔥 COMMON FIELD MAPPING
                employ.first_name = employee.Firstname;
                employ.last_name = employee.Lastname;
                employ.department_id = employee.departmentid;
                employ.designation_id = employee.designationid;
                employ.dob = employee.dob;
                employ.joining_date = employee.joining_date;
                employ.email = employee.email;
                employ.phone = employee.phone;
                employ.address = employee.address;
                employ.status = employee.status;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return employee;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
