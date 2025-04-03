namespace DoAn.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;

    public partial class AdminUser
    {
        public enum UserRole
        {
            Admin,
            Employee
        }

        [Display(Name = "Mã User")]
        public int ID { get; set; }

        [Required(ErrorMessage = "Name not empty ...")]
        [Display(Name = "Tên User")]
        public string NameUser { get; set; }

        [DisplayName("Vị Trí")]
        public string RoleUser { get; set; }

        [DisplayName("Nhập Mật Khẩu")]
        [Required(ErrorMessage = "Pass not empty ...")]
        [DataType(DataType.Password)]
        public string PasswordUser { get; set; }

        [NotMapped]
        public string Errorlogin { get; set; }

        // You can also add this to get the role list for dropdown
        public static List<SelectListItem> RoleList => new List<SelectListItem>
        {
            new SelectListItem { Text = "Admin", Value = "Admin" },
            new SelectListItem { Text = "Employee", Value = "Employee" }

        };
    }
}
