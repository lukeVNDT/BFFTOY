using System;
using System.Collections.Generic;

namespace Tfood.Models
{
    public partial class Shipper
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public DateTime? ShipDate { get; set; }
    }
}
