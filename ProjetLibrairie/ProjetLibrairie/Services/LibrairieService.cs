using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetLibrairie.ExceptionHandler;
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
            Dictionary<string, int> bookDictionnary = ItemsQuantity(basketByNames);

            if (basketByNames.Length != 0)
            {
                //prix quand il y a un seul livre
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
                //prix quand il y a plusieurs livres
                else
                {
                    IEnumerable<IGrouping<string, Orders>> ListOfBookByCategory = ConstructBookFromName(basketByNames, bookDictionnary).GroupBy(x => x.Book.Category);
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
            //vérification si le livre existe
            return _libraryRepository.Catalog.Find(x => x.Name == name);
        }

        private List<Orders> ConstructBookFromName(string[] basketByNames, Dictionary<string, int> bookDictionnary)
        {
            List<Orders> MyOrders = new List<Orders>();
            List<Book> listofbook = _libraryRepository.Catalog.Where(x => basketByNames.Contains(x.Name)).ToList();
            //construction de l'objet Order à partir en fonction des noms de livre passé en paramètre           
            foreach (Book book in listofbook)
            {
                Orders MyOrder = new Orders();
                MyOrder.Book = book;
                MyOrder.QuantityOrder = bookDictionnary[book.Name];
                //vérification de la quantité disponible
                CheckQuantityOrder(MyOrder);
                MyOrders.Add(MyOrder);
            }
            //exception levée lorsqu'il n'y a pas assez de livre disponible
            if (nonAvailableBookName.Count() != 0)
            {
                throw new NotEnoughInventoryException(nonAvailableBookName);
            }
            return MyOrders;

        }

        private Dictionary<string, int> ItemsQuantity(string[] basketByNames)
        {

            Dictionary<string, int> items = new Dictionary<string, int>();
            List<string> distinctName = basketByNames.Distinct().ToList();
            //identification de la quantité de commande pour chaque livre distinct
            foreach (string value in distinctName)
            {
                int count = basketByNames.Count(x => x == value);
                items.Add(value, count);
            }
            return items;
        }

        private void CheckQuantityOrder(Orders nonAvailableBook)
        {
            //vérification s'il y'a assez de livre disponible par rapport à la commande
            if (nonAvailableBook.QuantityOrder > nonAvailableBook.Book.Quantity)
            {
                nonAvailableBookName.Add(nonAvailableBook.Book);
            }
        }

        private void UpdateCatalog(Dictionary<string, int> bookDictionnary)
        {
            foreach (string key in bookDictionnary.Keys)
            {
                Book bookName = FindBookByName(key);
                int value = bookDictionnary[key];
                //mise à jour des quantité de livre pour chaque élément du catalogue àpres l'achat
                _libraryRepository.Catalog.Where(x => x == bookName).FirstOrDefault().Quantity = bookName.Quantity - value;
            }
        }

        private double PriceWhenDifferentCategoryAndManyQuantity(IEnumerable<IGrouping<string, Orders>> ListOfBookByCategory)
        {
            double price = 0;
            foreach (var listofbooks in ListOfBookByCategory)
            {
                string catname = listofbooks.Key;
                //recupération du discount appliqué à la catégorie
                double discount = _libraryRepository.Category.Where(x => x.Name == catname).Select(x => x.Discount).FirstOrDefault();
                foreach (Orders item in listofbooks)
                {
                    //cas ou il y'a plusieurs livre identique ou de la même catégorie
                    if (item.QuantityOrder > 1 || listofbooks.Count() > 1)
                    {
                        for (int i = 0; i < item.QuantityOrder; i++)
                        {
                            //aplication du discount sur un des éléments de la commande
                            if (i == 0)
                            {
                                price += item.Book.Price * (1 - discount);
                            }
                            else
                            {
                                price += item.Book.Price;
                            }
                        }
                    }
                    //cas ou il y'a un livre par catégorie
                    else
                    {
                        price += listofbooks.FirstOrDefault().Book.Price;
                    }
                }
            }
            return price;
        }

    }
}
