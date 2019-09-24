using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetLibrairie.Models;
using ProjetLibrairie.Repositories;
using ProjetLibrairie.Repositories.Interfaces;
using ProjetLibrairie.Services.Interfaces;
using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetLibrairie.Services
{
    public class LibrairieService : IStore
    {
        private LibrairieVM Librairie;
        Book LibrairieBook;
        private ILibrairieRepository _libraryRepository;
        private List<INameQuantity> nonAvailableBookName;

        public LibrairieService(ILibrairieRepository lib)
        {
            LibrairieBook = new Book();
            nonAvailableBookName = new List<INameQuantity>();
            _libraryRepository = lib;
        }

        public double Buy(params string[] basketByNames)
        {
            double price = 0;
            var bookDictionnary = ItemsQuantity(basketByNames);

            if (basketByNames.Length != 0)
            {
                if (basketByNames.Length == 1)
                {
                    LibrairieBook = FindBookByName(basketByNames[0]);
                    if (LibrairieBook != null)
                    {
                        price = LibrairieBook.Price;
                        UpdateCatalog(bookDictionnary);
                        return price;
                    }
                }
                else
                {
                    var ListOfBookByCategory = ConstructBookFromName(basketByNames, bookDictionnary).GroupBy(x => x.Category);
                    price = PriceWhenDifferentCategoryAndManyQuantity(ListOfBookByCategory);
                    UpdateCatalog(bookDictionnary);
                    return price;

                }
            }

            return price;
        }

        public void Import(string catalogAsJson)
        {
            Librairie = JsonConvert.DeserializeObject<LibrairieVM>(catalogAsJson);
            _libraryRepository.InitializeRepo(Librairie.Catalog, Librairie.Category);
        }

        public int Quantity(string name)
        {
            int quantity = 0;

            LibrairieBook = FindBookByName(name);
            if (LibrairieBook != null)
                quantity = LibrairieBook.Quantity;

            return quantity;
        }

        private Book FindBookByName(string name)
        {
            return _libraryRepository.Catalog.Find(x => x.Name == name);
        }

        private List<Book> ConstructBookFromName(string[] basketByNames, Dictionary<string, int> bookDictionnary)
        {
            List<Book> MyOrder = new List<Book>();
            var listofbook = _libraryRepository.Catalog.Where(x => basketByNames.Contains(x.Name)).ToList();
            foreach (var book in listofbook)
            {
                book.QuantityOrder = bookDictionnary[book.Name];
                CheckQuantityOrder(book);
                MyOrder.Add(book);
            }
            if (nonAvailableBookName.Count() != 0)
            {
                throw new NotEnoughInventoryException(nonAvailableBookName);
            }
            return MyOrder;

        }

        private Dictionary<string, int> ItemsQuantity(string[] basketByNames)
        {
            Dictionary<string, int> items = new Dictionary<string, int>();
            var dic = basketByNames.Distinct().ToList();
            foreach (var value in dic)
            {
                int count = basketByNames.Count(x => x == value);
                items.Add(value, count);
            }
            return items;
        }

        private void CheckQuantityOrder(Book nonAvailableBook)
        {
            if (nonAvailableBook.QuantityOrder > nonAvailableBook.Quantity)
            {
                nonAvailableBookName.Add(nonAvailableBook);
            }
        }

        private void UpdateCatalog(Dictionary<string, int> bookDictionnary)
        {
            foreach (var key in bookDictionnary.Keys)
            {
                var b = FindBookByName(key);
                var value = bookDictionnary[key];
                _libraryRepository.Catalog.Where(x => x == b).FirstOrDefault().Quantity = b.Quantity - value;
            }
        }

        private double PriceWhenDifferentCategoryAndManyQuantity(IEnumerable<IGrouping<string, Book>> ListOfBookByCategory)
        {
            double price = 0;
            foreach (var listofbooks in ListOfBookByCategory)
            {
                string catname = listofbooks.Key;
                double discount = _libraryRepository.Category.Where(x => x.Name == catname).Select(x => x.Discount).FirstOrDefault();
                foreach (var item in listofbooks)
                {
                    if (item.QuantityOrder > 1 || listofbooks.Count() > 1)
                    {
                        for (int i = 0; i < item.QuantityOrder; i++)
                        {
                            if (i == 0)
                            {
                                price += item.Price * (1 - discount);
                            }
                            else
                            {
                                price += item.Price;
                            }
                        }

                    }
                    else
                    {
                        price += listofbooks.FirstOrDefault().Price;
                    }

                }
            }

            return price;
        }

    }
}
