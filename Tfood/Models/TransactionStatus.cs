using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class TransactionStatus
    {
        public TransactionStatus()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
