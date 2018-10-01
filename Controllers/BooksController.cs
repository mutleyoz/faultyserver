using FaultyServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace FaultyServer.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController: Controller
    {
        private readonly ILogger _logger;
        private readonly BooksContext _context;
        private static volatile int Counter;

        public BooksController(ILogger<BooksController> logger, BooksContext context)
        {
            _logger = logger;
            _context = context;

            if(_context.Books.Count() == 0)
            {
                _context.Books.Add( new Book { Title = "The Dark Tower", Author = "Stephen King" });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetAll()
        {
            _logger.LogInformation("GetAll");

            if(Interlocked.Increment(ref Counter) % 2 == 0)
            {
                await Task.Delay(3000);
                return StatusCode(500);
            }
            return _context.Books.ToList();
        }

        [HttpGet("{id}", Name = "GetBook")]
        [ProducesResponseType(200, Type = typeof(Book))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Book>> GetById(long id)
        {
            var book = await _context.Books.FindAsync(id);
            if(book == null)
            {
                _logger.LogWarning($"GetById: book with id: {id} was not found");
                return NotFound();
            }
            return Ok(book);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Book))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateAsync([FromBody] Book book)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            _logger.LogInformation($"CreateAsync: {book}");
            await _context.Books.AddAsync(book).ConfigureAwait(false);
            _context.SaveChanges();

            return CreatedAtAction (nameof(GetById), new { id = book.Id}, book);
        }
    }
}