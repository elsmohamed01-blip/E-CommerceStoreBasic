using System;
using System.Collections.Generic;

#nullable disable

namespace E_Commerce.Models
{
    public partial class ProductImage
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string Image { get; set; }

        public virtual Product Product { get; set; }
    }
}
