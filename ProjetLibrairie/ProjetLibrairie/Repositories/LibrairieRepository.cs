using ProjetLibrairie.Models;
using ProjetLibrairie.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetLibrairie.Repositories
{
    public class LibrairieRepository : ILibrairieRepository
    {
        public List<Category> Category { get; set; }
        public List<Book> Catalog { get; set; }

        public void InitializeRepo(List<Book> catalog, List<Category> category)
        {
            //creation de la base de donnée
            Category = category;
            Catalog = catalog;
        }
    }
}
