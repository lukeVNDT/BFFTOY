using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class Page
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Contents { get; set; } = null!;
        public string? Thumb { get; set; }
        public bool Published { get; set; }
        public string? Title { get; set; }
        public string? MetaDesc { get; set; }
        public string? MetaKey { get; set; }
        public string? Alias { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Sequence { get; set; }
        public string Icon { get; set; } = null!;
    }
}
