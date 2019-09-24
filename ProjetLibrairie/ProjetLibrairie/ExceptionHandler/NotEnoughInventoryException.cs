using ProjetLibrairie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetLibrairie.ExceptionHandler
{
    public class NotEnoughInventoryException : Exception
    {
         
        public NotEnoughInventoryException(List<INameQuantity>books)
        {
            Missing = books;
        }
        public IEnumerable<INameQuantity> Missing { get; }
    }
}
