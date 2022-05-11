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
    public class CategoriesController : Controller
    {
        private DBConnection db = new DBConnection();

        // GET: Categories
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder; //PageList
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            
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
             var categories = db.Categories.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(s => s.Name.Contains(searchString));
            }

            //Search by ElasticSearch
            var categoriesList = db.Categories.AsQueryable().ToList();
            var searchRequest = new SearchRequest<Category>()
            {
                From = 0,
                Size = 10,
                QueryOnQueryString = searchString
            };
            var searchResult = ElasticSearchService.GetInstance().Search<Category>(searchRequest);
            if (!String.IsNullOrEmpty(searchString))
            {
                categoriesList = searchResult.Documents.ToList();
            }

            //Order by Name
            switch (sortOrder)
            {
                case "name_desc":
                    categories = categories.OrderBy(s => s.Name);
                    break;
                default:
                    categories = categories.OrderBy(s => s.Id);
                    break;
            }


            int pageSize = 5;
            int pageNumber = (page ?? 1);

            //list by linq
            //return View(articles.ToPagedList(pageNumber, pageSize));

            //list by elastic
            return View(categories.ToPagedList(pageNumber, pageSize));
        }



        public ActionResult IndexAjax(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder; //PageList
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

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
            var categories = db.Categories.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(s => s.Name.Contains(searchString));
            }

            //Search by ElasticSearch
            var categoriesList = db.Categories.AsQueryable().ToList();
            var searchRequest = new SearchRequest<Category>()
            {
                From = 0,
                Size = 10,
                QueryOnQueryString = searchString
            };
            var searchResult = ElasticSearchService.GetInstance().Search<Category>(searchRequest);
            if (!String.IsNullOrEmpty(searchString))
            {
                categoriesList = searchResult.Documents.ToList();
            }

            //Order by Name
            switch (sortOrder)
            {
                case "name_desc":
                    categories = categories.OrderBy(s => s.Name);
                    break;
                default:
                    categories = categories.OrderBy(s => s.Id);
                    break;
            }


            int pageSize = 5;
            int pageNumber = (page ?? 1);

            //list by linq
            //return View(articles.ToPagedList(pageNumber, pageSize));

            //list by elastic
            return PartialView(categories.ToPagedList(pageNumber, pageSize));
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ElasticSearchService.GetInstance().IndexDocument(category);
                    db.Categories.Add(category);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
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
