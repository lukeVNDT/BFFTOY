using System.ComponentModel.DataAnnotations;

namespace Tfood.ViewModel
{
    public class CheckoutVM
    {
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Họ và Tên")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Địa chỉ nhận hàng")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ghi chú")]
        public string Note { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public byte OrderMethod { get; set; }
    }
}
