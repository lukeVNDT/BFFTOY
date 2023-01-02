using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;


namespace Tfood.ViewModel
{
    public class RegisterVM
    {
        [Key]
        public int Id { get; set; }

        [Display(Name ="Họ và tên")]
        [Required(ErrorMessage ="Vui lòng nhập họ tên")]
        public string Fullname { get; set; }

        [MaxLength(150)]
        [Required(ErrorMessage ="Vui lòng nhập Email")]
        [DataType(DataType.EmailAddress)]
        [Remote(action:"ValidateEmail", controller:"Customer")]
        public string Email { get; set; }


        [MaxLength(11)]
        [Required(ErrorMessage ="Vui lòng nhập số điện thoại")]
        [DataType(DataType.PhoneNumber)]
        [Remote(action:"ValidatePhone", controller:"Customer")]
        public string Phone { get; set; }


        [Display(Name ="Mật khẩu")]
        [Required(ErrorMessage ="Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage ="Mật khẩu phải tối thiểu 6 ký tự")]
        public string Password { get; set; }


        [Display(Name = "Nhập lại mật khẩu")]
        [Compare("Password", ErrorMessage ="Mật khẩu không khớp, vui lòng kiểm tra lại")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải tối thiểu 6 ký tự")]
        public string PasswordConfirm { get; set; }

    }
}
