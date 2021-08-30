 using ApiSearch.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiSearch.Services
{
    public class BookService
    {
        private readonly IMongoCollection<Book> _books;

        public BookService(IBookstoreDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _books = database.GetCollection<Book>(settings.BooksCollectionName);
        }

        public List<Book> Get() =>
            _books.Find(book => true).ToList();

        public Book Get(string id) =>
            _books.Find<Book>(book => book.Id == id).FirstOrDefault();

        public Book Create(Book book)
        {
            _books.InsertOne(book);
            return book;
        }

        public void Update(string id, Book bookIn) =>
            _books.ReplaceOne(book => book.Id == id, bookIn);

        public void Remove(Book bookIn) =>
            _books.DeleteOne(book => book.Id == bookIn.Id);

        public void Remove(string id) =>
            _books.DeleteOne(book => book.Id == id);

        public Object Query(string s, string sort, int? queryPage)
        {
            var filter = Builders<Book>.Filter.Empty;

            if (!string.IsNullOrEmpty(s))
            {
                filter = Builders<Book>.Filter.Regex("BookName", new BsonRegularExpression(s, "i")) |
                    Builders<Book>.Filter.Regex("Author", new BsonRegularExpression(s, "i"));
            }

            var find = _books.Find(filter);
            find = (sort == "asc") ? find.SortBy(p => p.Price) : find.SortByDescending(p => p.Price);
            int page = queryPage.GetValueOrDefault(1);
            var total = find.CountDocuments();
            page = (page == 0) ? 1 : page;
            int perPage = 10;

            return new
            {
                data = find.Skip((page - 1) * perPage).Limit(perPage).ToList(),
                total = total,
                perPage = perPage,
                page = page,
                last_page = Math.Ceiling((double)total/perPage)
            };
        }
    }
}
