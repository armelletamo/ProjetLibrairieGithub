using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetLibrairie.Models
{
    public class LibrairieVM
    {
        public List<Category> Category { get; set; }
        public List<Book> Catalog { get; set; }
    }
}
