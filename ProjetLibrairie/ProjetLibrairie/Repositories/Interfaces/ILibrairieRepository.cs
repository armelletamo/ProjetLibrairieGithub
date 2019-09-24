using ProjetLibrairie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetLibrairie.Repositories.Interfaces
{
    public interface ILibrairieRepository
    {
        List<Category> Category { get; set; }
        List<Book> Catalog { get; set; }
        void InitializeRepo(List<Book> catalog, List<Category> category);
    }
}
