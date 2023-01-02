using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string ShortDesc { get; set; } = null!;
        public string Contents { get; set; } = null!;
        public string? Thumb { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool Published { get; set; }
        public string Author { get; set; } = null!;
        public int? AccountId { get; set; }
        public bool IsHot { get; set; }
        public bool IsNewFeed { get; set; }
        public int? CatId { get; set; }
        public string? Alias { get; set; }
        public string? MetaDesc { get; set; }
        public string? MetaKey { get; set; }

        public virtual Account? Account { get; set; }
        public virtual PostCategory? Cat { get; set; }
    }
}
