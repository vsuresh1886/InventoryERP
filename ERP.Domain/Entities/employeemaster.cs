using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("employee_master")]
    public class employee_master
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public Int32 employee_pk { get; set; }
            public string? employee_id { get; set; }
            public string? first_name { get; set; }
            public string? last_name { get; set; }
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
            public DateTime? created_at { get; set; }
            public DateTime? updated_at { get; set; }
            public string? password { get; set; }
            public string? profile_img { get; set; }

        }
    }
