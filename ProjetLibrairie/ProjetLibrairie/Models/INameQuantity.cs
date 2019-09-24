using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetLibrairie.Models
{
    public interface INameQuantity
    {
        string Name { get; }
        int Quantity { get; }
    }
}
