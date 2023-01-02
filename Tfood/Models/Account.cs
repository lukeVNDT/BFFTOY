using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class Account
    {
        public Account()
        {
            Posts = new HashSet<Post>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Fullname { get; set; }
        public int? RoleId { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? Salt { get; set; }
        public byte? Deactivate { get; set; }

        public virtual Role? Role { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
