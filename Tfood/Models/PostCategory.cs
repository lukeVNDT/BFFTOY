using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class PostCategory
    {
        public PostCategory()
        {
            Posts = new HashSet<Post>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Thumb { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
}
