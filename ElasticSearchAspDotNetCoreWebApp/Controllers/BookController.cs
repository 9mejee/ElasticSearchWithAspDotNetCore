using ElasticSearch.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;

namespace ElasticSearch.Controllers
{
    public class BookController : Controller
    {
        private readonly IElasticClient _elasticClient;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BookController(IElasticClient elasticClient, IWebHostEnvironment hostingEnvironment)
        {
            _elasticClient = elasticClient;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public ActionResult Index(string keyword)
        {
            var BookList = new List<BookModel>();
            if (!string.IsNullOrEmpty(keyword))
            {
                BookList = GetSearch(keyword).ToList();
            }

            return View(BookList.AsEnumerable());
        }

        public IList<BookModel> GetSearch(string keyword)
        {
            var result = _elasticClient.SearchAsync<BookModel>(
                s => s.Query(
                    q => q.QueryString(
                        d => d.Query('*' + keyword + '*')
                    )).Size(5000));

            var finalResult = result;
            var finalContent = finalResult.Result.Documents.ToList();
            return finalContent;
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookModel model)
        {
            try
            {
                var Book = new BookModel()
                {
                    Id = 1,
                    Title = model.Title,
                    Link = model.Link,
                    Date = DateTime.Now
                };

                await _elasticClient.IndexDocumentAsync(Book);
                model = new BookModel();
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(BookModel model)
        {
            try
            {
                var Book = new BookModel()
                {
                    Id = 1,
                    Title = model.Title,
                    Link = model.Link,
                    Date = DateTime.Now
                };

                await _elasticClient.DeleteAsync<BookModel>(Book);
                model = new BookModel();
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import()
        {
            try
            {
                var rootPath = _hostingEnvironment.ContentRootPath; //get the root path

                var fullPath =
                    Path.Combine(rootPath,
                        "Books.json"); //combine the root path with that of our json file inside mydata directory

                var jsonData = System.IO.File.ReadAllText(fullPath); //read all the content inside the file

                var BookList = JsonConvert.DeserializeObject<List<BookModel>>(jsonData);
                if (BookList != null)
                {
                    foreach (var Book in BookList)
                    {
                        _elasticClient.IndexDocumentAsync(Book);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Index");
        }

    }
}