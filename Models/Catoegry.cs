using System;
using System.Collections.Generic;

#nullable disable

namespace E_Commerce.Models
{
    public partial class Catoegry
    {
        public Catoegry()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
