using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class EmployeeDto
    {
        public int Sno { get; set; }
        public int Id { get; set; }
        public string? Employee_Code { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public int Designation_Id { get; set; }
        public string? Role { get; set; }
        public int Department_Id { get; set; }
        public string? Department { get; set; }
        public string? Status { get; set; }
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? Actions { get; set; }
    }

    public class EmployeeDetailDto
    {
        public int Id { get; set; }
        public string? Employee_Code { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? gender { get; set; }
        public DateTime? dob { get; set; }
        public DateTime? joining_date { get; set; }
        public Int32 department_id { get; set; }
        public Int32 designation_id { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? status { get; set; }
    }

    public class EmployeeSaveDto
    {
        public int Id { get; set; }
        public string? Employee_Code { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? gender { get; set; }
        public DateTime? dob { get; set; }
        public DateTime? joining_date { get; set; }
        public Int32 departmentid { get; set; }
        public Int32 designationid { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? status { get; set; }
    }


}
