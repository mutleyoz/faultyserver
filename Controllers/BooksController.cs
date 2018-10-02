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
        public BooksController(ILogger<BooksController> logger, BooksContext context)
        {
            _logger = logger;
            _context = context;

            if(_context.Books.Count() == 0)
            {
                _context.Books.Add( new Book { Title = "The Dark Tower", Author = "Stephen King" });
                _context.Books.Add( new Book { Title = "The Thief of Always", Author = "Clive Barker" });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public ActionResult<List<Book>> GetAll()
        {
            _logger.LogInformation("{nameof(this.Name)}");

            if(( DateTime.Now.Second % 10 ) > 5)
            {
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