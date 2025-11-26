using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce.Models
{
    public class IndexVM
    {
        public IndexVM()
        {
            Cateogries = new List<Catoegry>();
            Products = new List<Product>();
            Reviews = new List<Review>();
            LatesProducts = new List<Product>();
        }
        public List<Catoegry> Cateogries { get; set; }
        public List<Product> LatesProducts { get; set; }
        public List<Product> Products { get; set; }
        public List<Review> Reviews { get; set; }
    }
}

