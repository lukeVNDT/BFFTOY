using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public string? Thumb { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
