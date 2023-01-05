﻿using ImpactAPI.Models;
using ImpactAPI.Repository;
using ImpactAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImpactAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ImpactContext _context;
        private ICurrencyService _currencyService;


        public ProductsController(ImpactContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var currency = await _currencyService.GetAllCurrency();

            if (currency is null)
            {
                return NotFound();
            }
            decimal USD = currency.USD.Ask;
            decimal EUR = currency.EUR.Ask;
            List<Product> products = await _context.Products.ToListAsync();
            products.ForEach(p => p.ConverteValue(USD, EUR));
            return products;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            var productFind = await _context.Products.FindAsync(id);
            if (productFind == null)
            {
                return NotFound();
            }
            Console.WriteLine(product.Price);

            try
            {
                productFind.Name = product.Name ?? productFind.Name;
                productFind.Price = product.Price != 0 ? product.Price : productFind.Price;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            var currency = await _currencyService.GetAllCurrency();

            if (currency is null)
            {
                return NotFound();
            }
            decimal USD = currency.USD.Ask;
            decimal EUR = currency.EUR.Ask;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            product.ConverteValue(USD, EUR);
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}