using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetLibrairie.Models;
using ProjetLibrairie.Services.Interfaces;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjetLibrairie
{
    [Route("librairie")]
    public class LibrairieController : Controller
        
    {
        private readonly IStore _librairieService;
        public LibrairieController(IStore store)
        {
            _librairieService = store;
        }

      
        [HttpPost]
        [Route("load")]
        public void LoadJsonFile(IFormFile files)
        {
            string result = "";
            if (files.FileName.EndsWith("json") && files.ContentType== "application/json")
            {
                byte[] content;
                using (var ms = new MemoryStream())
                {
                    files.CopyTo(ms);
                    content = ms.ToArray();
                }
                result = System.Text.Encoding.UTF8.GetString(content);
            }

            _librairieService.Import(result);
        }

        [HttpGet]
        [Route("getquantity")]
        public int GetQuantity(string name)
        {
            int quantity=_librairieService.Quantity(name);
            return quantity;
        }

        [HttpPost]
        [Route("Buy")]
        [ProducesResponseType(typeof(double), 200)]
        [ProducesResponseType(typeof(List<INameQuantity>), 400)]
        public IActionResult GetPrice([FromBody] BooksVM bookNames)
        {
            try
            {
                double price = _librairieService.Buy(bookNames.BookNames);
                return Ok(price);
            }
            catch(NotEnoughInventoryException ex)
            {
               return BadRequest(ex.Missing.ToList());
            }
        }
    }
    
}
