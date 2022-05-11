using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AdminCrawler.Data;
using AdminCrawler.Models;
using AdminCrawler.Services;
using Nest;
using PagedList;

namespace AdminCrawler.Controllers
{
    public class ArticlesController : Controller
    {
        private DBConnection db = new DBConnection();

        // GET: Articles
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder; //PageList
            ViewBag.TagSortParm = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.CateSortParm = String.IsNullOrEmpty(sortOrder) ? "cate_desc" : "";
            //PageList + search ajax
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            //Search by Linq
            var articles = db.Articles.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                articles = articles.Where(s => s.Title.Contains(searchString) ||
                                          s.Category.Contains(searchString)   );
            }

            //Search by ElasticSearch
            var articleList = db.Articles.AsQueryable().ToList();
            var searchRequest = new SearchRequest<Article>()
            {
                From = 0,
                Size = 10,
                QueryOnQueryString = searchString
            };
            var searchResult = ElasticSearchService.GetInstance().Search<Article>(searchRequest);
            if (!String.IsNullOrEmpty(searchString))
            {
                articleList = searchResult.Documents.ToList();
            }

            //Order by Name
            switch (sortOrder)
            {
                case "title_desc":
                    articles = articles.OrderBy(s => s.Title);
                    break;
                case "cate_desc":
                    articles = articles.OrderBy(s => s.Category);
                    break;
                default:
                    articles = articles.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            articles = db.Articles.Include(a => a.Source);
            return View(articleList.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult IndexAjax(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder; //PageList
            ViewBag.TagSortParm = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.CateSortParm = String.IsNullOrEmpty(sortOrder) ? "cate_desc" : "";
            //PageList + search ajax
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            //Search by Linq
            var articles = db.Articles.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                articles = articles.Where(s => s.Title.Contains(searchString) ||
                                          s.Category.Contains(searchString));
            }

            //Search by ElasticSearch
            var articleList = db.Articles.AsQueryable().ToList();
            var searchRequest = new SearchRequest<Article>()
            {
                From = 0,
                Size = 10,
                QueryOnQueryString = searchString
            };
            var searchResult = ElasticSearchService.GetInstance().Search<Article>(searchRequest);
            if (!String.IsNullOrEmpty(searchString))
            {
                articleList = searchResult.Documents.ToList();
            }

            //Order by Name
            switch (sortOrder)
            {
                case "title_desc":
                    articles = articles.OrderBy(s => s.Title);
                    break;
                case "cate_desc":
                    articles = articles.OrderBy(s => s.Category);
                    break;
                default:
                    articles = articles.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            articles = db.Articles.Include(a => a.Source);
            return PartialView(articleList.ToPagedList(pageNumber, pageSize));
        }

        // GET: Articles/Details/5
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

        // GET: Articles/Create
        public ActionResult Create()
        {
            ViewBag.SourceId = new SelectList(db.Sources, "Id", "Url");
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UrlSource,Title,Image,Description,Content,Category,CreatedAt,SourceId")] Article article)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ElasticSearchService.GetInstance().IndexDocument(article);
                    db.Articles.Add(article);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            ViewBag.SourceId = new SelectList(db.Sources, "Id", "Url", article.SourceId);
            return View(article);
        }

        // GET: Articles/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.SourceId = new SelectList(db.Sources, "Id", "Url", article.SourceId);
            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UrlSource,Title,Image,Description,Content,Category,CreatedAt,SourceId")] Article article)
        {
            if (ModelState.IsValid)
            {
                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SourceId = new SelectList(db.Sources, "Id", "Url", article.SourceId);
            return View(article);
        }

        // GET: Articles/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Article article = db.Articles.Find(id);
            db.Articles.Remove(article);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
