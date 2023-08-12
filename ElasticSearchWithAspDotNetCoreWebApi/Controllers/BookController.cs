using Newtonsoft.Json;

namespace Elastic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class BookController : ControllerBase
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<BookController> _logger;
        public BookController(IElasticClient elasticClient, ILogger<BookController> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        [Route("import")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import()
        {
            try
            {
                var fullPath = $"{Directory.GetCurrentDirectory()}/Books.json"; //combine the root path with that of our json file inside mydata directory
                var jsonData = System.IO.File.ReadAllText(fullPath); //read all the content inside the file
                var BookList = JsonConvert.DeserializeObject<List<BookModel>>(jsonData);
                if (BookList != null)
                {
                    foreach (var Book in BookList)
                    {
                        _elasticClient.IndexDocumentAsync(Book);
                    }
                }
                return Ok("Done");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("create")]
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
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("update")]
        [HttpPost]
        public ActionResult Update(BookModel model) => Ok(_elasticClient.UpdateAsync<BookModel>(model.Id, u => u.Index("Books").Doc(model)));

        [Route("delete")]
        [HttpPost]
        public ActionResult Delete(BookModel model) => Ok(_elasticClient.DeleteAsync<BookModel>(model));


        [Route("search")]
        [HttpGet]
        public ActionResult search(string keyword)
        {
            var BookList = new List<BookModel>();
            if (!string.IsNullOrEmpty(keyword))
                BookList = GetSearch(keyword).ToList();

            return Ok(BookList.AsEnumerable());
        }

        private IList<BookModel> GetSearch(string keyword)
        {
            var result = _elasticClient
                .SearchAsync<BookModel>(search => search.Query(query => query.QueryString(selector => selector.Query('*' + keyword + '*')))
                .Size(5000));
            var finalResult = result;
            return finalResult.Result.Documents.ToList();
        }
    }
}