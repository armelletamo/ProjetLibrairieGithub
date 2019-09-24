using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetLibrairie.Models
{
    public class Book : INameQuantity
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }

    }
}
