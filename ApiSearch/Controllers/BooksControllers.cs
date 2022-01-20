using ApiSearch.Models;
using ApiSearch.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly IDatabase _redis;

        public BooksController(BookService bookService, IDatabase redis)
        {
            _bookService = bookService;
            _redis = redis;
        }

        [HttpGet]
        public ActionResult<List<Book>> Get() =>
            _bookService.Get();

        [HttpGet("{id:length(24)}", Name = "GetBook")]
        public ActionResult<Book> Get(string id)
        {
            var book = _bookService.Get(id);
            return book != null ? book : NotFound();
        }

        [HttpPost]
        public ActionResult<Book> Create(Book book)
        {
            _bookService.Create(book);
            return CreatedAtRoute("GetBook", new { id = book.Id.ToString() }, book);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Book bookIn)
        {
            var book = _bookService.Get(id);
            if (book == null)
            {
                return NotFound();
            }
            _bookService.Update(id, bookIn);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var book = _bookService.Get(id);
            if (book == null)
            {
                return NotFound();
            }
            _bookService.Remove(book.Id);
            return NoContent();
        }

        [HttpGet("search")]
        public ActionResult Search(
            [FromQuery(Name = "s")] string s,
            [FromQuery(Name = "sort")] string sort,
            [FromQuery(Name = "page")] int page
        )
        {
            string key = $"{s}_{sort}_{page.ToString()}";
            var data = _redis.StringGet(key);
            if (data.HasValue) {
                return Ok(data.ToString());
            }
            var query = _bookService.Query(s, sort, page);
            _redis.StringSet(key, JsonConvert.SerializeObject(query));
            return Ok(query);
        }

    }
}