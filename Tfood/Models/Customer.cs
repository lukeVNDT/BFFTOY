using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Favorites = new HashSet<Favorite>();
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public string? Phone { get; set; }
        public DateTime? Dob { get; set; }
        public int? LocationId { get; set; }
        public int? District { get; set; }
        public int? Ward { get; set; }
        public string? Avatar { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? Salt { get; set; }
        public string? Address { get; set; }
        public byte? Deactivate { get; set; }

        public virtual Location? Location { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
