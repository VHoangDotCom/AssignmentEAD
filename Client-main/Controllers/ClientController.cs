using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClientNews.Data;
using ClientNews.Models;
using ClientNews.Services;
using Nest;
using PagedList;

namespace ClientNews.Controllers
{
    public class ClientController : Controller
    {
        private MyDbContext db = new MyDbContext();
        // GET: Client
        public ActionResult Index(string searchString, int? page)
        {

           var  articles = db.Articles.ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                
                var elasticSearchKeyWord = ElasticSearchQuery(searchString).ToArray();
                if (elasticSearchKeyWord != null)
                {
                    articles = articles.Where(s => elasticSearchKeyWord.Contains(s.Id)).ToList();
                }
               
            }
            @ViewBag.CurrentFilter = searchString;
             ViewData["category"] = db.Categories.ToList();

            //Count News
            ViewData["category_0"] = db.Categories.ToList();


            int countCate;

            foreach (var line in articles.GroupBy(n => n.Id)
                                         .Select(n => new
                                         {
                                             Category = n.Key,
                                             Count = n.Count()
                                         })
                                         .OrderBy(n => n.Category))
            {
                countCate = line.Count;
                ViewData["count"] = Convert.ToInt32(countCate);
            }
                                       

   
          
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(articles.ToPagedList(pageNumber, pageSize));
        }

        // GET: Client/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Articles.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }          
            return View(article);
        }

        public ActionResult ElasticDeleteIndex()
        {
            ElasticSearchService.GetInstance().DeleteByQueryAsync<Article>(s =>
        s.Query(q => q.MatchAll()));
            return View("Success");
        }

        public ActionResult ElasticSearchLoad()
        {
            List<Article> listArticle = db.Articles.ToList();
            //    ElasticSearchService.GetInstance().DeleteByQueryAsync<Article>(s =>
            //s.Query(q => q.MatchAll()));


            foreach (Article article in listArticle)
            {
                var articleNew = new Article()
                {
                    Id = article.Id,
                    UrlSource = article.UrlSource,
                    Title = article.Title,
                    Description = article.Description,
                    Content = article.Content,
                    Image = article.Image,
                    Category = article.Category,   
                };
                ElasticSearchService.GetInstance().IndexDocument(articleNew);
            }

            return View("Success");
        }

        List<int> ElasticSearchQuery(string searchString)
        {
            var list = new List<Article>();
            var listId = new List<int>();
            var searchRequest = new SearchRequest<Article>()
            {
                QueryOnQueryString = searchString,
            };
            var searchResult = ElasticSearchService.GetInstance().Search<Article>(searchRequest);
            list = searchResult.Documents.ToList();
            foreach (Article article in list)
            {
                listId.Add(article.Id);
            }
            return listId;
        }
    }
}
