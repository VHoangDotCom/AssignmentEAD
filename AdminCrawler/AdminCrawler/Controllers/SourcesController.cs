using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AdminCrawler.Data;
using AdminCrawler.Models;
using AdminCrawler.Services;
using HtmlAgilityPack;
using Nest;
using PagedList;

namespace AdminCrawler.Controllers
{
    public class SourcesController : Controller
    {
        private DBConnection db = new DBConnection();
        public static HashSet<Source> setLink;
        // GET: Sources
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder; //PageList
            ViewBag.TagSortParm = String.IsNullOrEmpty(sortOrder) ? "tag_desc" : "";

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
            var sources = db.Sources.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                sources = sources.Where(s => s.Tag.Contains(searchString) ||
                                          s.SelectorTitle.Contains(searchString) ||
                                          s.SelectorDescription.Contains(searchString));
            }

            //Search by ElasticSearch
           /* var sourcesList = db.Sources.AsQueryable().ToList();
            var searchRequest = new SearchRequest<Source>()
            {
                From = 0,
                Size = 10,
                QueryOnQueryString = searchString
            };
            var searchResult = ElasticSearchService.GetInstance().Search<Source>(searchRequest);
            if (!String.IsNullOrEmpty(searchString))
            {
                sourcesList = searchResult.Documents.ToList();
            }*/

            //Order by Name
            switch (sortOrder)
            {
                case "tag_desc":
                    sources = sources.OrderBy(s => s.Tag);
                    break;
                default:
                    sources = sources.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            //list by elastic
            return View(sources.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult IndexAjax(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder; //PageList
            ViewBag.TagSortParm = String.IsNullOrEmpty(sortOrder) ? "tag_desc" : "";

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
            var sources = db.Sources.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                sources = sources.Where(s => s.Tag.Contains(searchString) ||
                                          s.SelectorTitle.Contains(searchString) ||
                                          s.SelectorDescription.Contains(searchString));
            }

            //Search by ElasticSearch
           /* var sourcesList = db.Sources.AsQueryable().ToList();
            var searchRequest = new SearchRequest<Source>()
            {
                From = 0,
                Size = 10,
                QueryOnQueryString = searchString
            };
            var searchResult = ElasticSearchService.GetInstance().Search<Source>(searchRequest);
            if (!String.IsNullOrEmpty(searchString))
            {
                sourcesList = searchResult.Documents.ToList();
            }*/

            //Order by Name
            switch (sortOrder)
            {
                case "tag_desc":
                    sources = sources.OrderBy(s => s.Tag);
                    break;
                default:
                    sources = sources.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            //list by elastic
            return PartialView(sources.ToPagedList(pageNumber, pageSize));
        }


        // GET: Sources/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Source source = db.Sources.Find(id);
            if (source == null)
            {
                return HttpNotFound();
            }
            return View(source);
        }

        // GET: Sources/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        // POST: Sources/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Url,Tag,SelectorSubUrl,SubUrl,SelectorTitle,SelectorImage,SelectorDescription,SelectorContent,CategoryId,ArticleId")] Source source)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ElasticSearchService.GetInstance().IndexDocument(source);
                    db.Sources.Add(source);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", source.CategoryId);
            return View(source);
        }

        // GET: Sources/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Source source = db.Sources.Find(id);
            if (source == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", source.CategoryId);
            return View(source);
        }

        // POST: Sources/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Url,Tag,SelectorSubUrl,SubUrl,SelectorTitle,SelectorImage,SelectorDescription,SelectorContent,CategoryId,ArticleId")] Source source)
        {
            if (ModelState.IsValid)
            {
                db.Entry(source).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", source.CategoryId);
            return View(source);
        }

        // GET: Sources/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Source source = db.Sources.Find(id);
            if (source == null)
            {
                return HttpNotFound();
            }
            return View(source);
        }

        // POST: Sources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Source source = db.Sources.Find(id);
            db.Sources.Remove(source);
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

        public ActionResult CheckSource()
        {
            return View();
        }
        public ActionResult PreviewLink()
        {
            ViewBag.ListCategory = db.Categories.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckLink(Source sourceCheck)
        {
            if (sourceCheck.Url != "" || sourceCheck.SelectorSubUrl != "")
            {

                try
                {
                    var web = new HtmlWeb();
                    HtmlDocument doc = web.Load(sourceCheck.Url);
                    var nodeList = doc.QuerySelectorAll(sourceCheck.SelectorSubUrl);//tìm đến những thẻ a nằm trong h3 có class= title-news
                    var setLinkSource = new HashSet<Source>(); // Đảm bảo link không giống nhau, nếu có sẽ bị ghi đè ở phần tử trước

                    foreach (var node in nodeList)
                    {
                        var link = node.Attributes["href"]?.Value;
                        if (string.IsNullOrEmpty(link)) // nếu link này null
                        {
                            continue;
                        }
                        var sourceCheck1 = new Source()
                        {
                            Url = link
                        };

                        setLinkSource.Add(sourceCheck1);
                    }
                    Debug.WriteLine("gia trị", sourceCheck.Url.ToString());
                    return PartialView("GetListSource", setLinkSource);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error ở đây: " + e.Message);
                    return PartialView("Faild");
                }
            }
            return PartialView("Faild");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Preview(Source sourceCheck, int category)
        {
            if (sourceCheck.SubUrl != "" && sourceCheck.SelectorTitle != "" && sourceCheck.SelectorDescription != ""
                && sourceCheck.SelectorContent != "" && sourceCheck.SelectorSubUrl != ""
                && sourceCheck.SelectorImage != "" )
            {
                try
                {
                    Console.OutputEncoding = System.Text.Encoding.UTF8;
                    var web = new HtmlWeb();
                    HtmlDocument doc = web.Load(sourceCheck.SubUrl);
                    var title = doc.QuerySelector(sourceCheck.SelectorTitle)?.InnerText ?? "";
                    var description = doc.QuerySelector(sourceCheck.SelectorDescription)?.InnerText ?? "";
                    var imageNode = doc.QuerySelector(sourceCheck.SelectorImage)?.Attributes["data-src"].Value;
                    var content = doc.QuerySelector(sourceCheck.SelectorContent)?.InnerText;
                    // var category = doc.QuerySelector(sourceCheck.CategoryId.ToString())?.InnerText ?? "";
                    string thumbnail = "";
                    if (imageNode != null)
                    {
                        thumbnail = imageNode;
                    }
                    else
                    {
                        thumbnail = "";
                    }

                    Article article = new Article()
                    {
                        Title = title,
                        Description = description,
                        Content = content,
                        Image = thumbnail,
                        //Category = db.Categories.Find(category).Name,

                    };

                    return PartialView("GetListSelector", article);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    return PartialView("Faild");
                }
            }
            return PartialView("Faild");
        }

        [HttpPost, ActionName("CreateSource")]
        public JsonResult CreateSource([Bind(Include = "Id,Url,Tag,SelectorSubUrl,SubUrl,SelectorTitle,SelectorImage,SelectorDescription,SelectorContent,CategoryId,ArticleId")] Source source)
        {
            try
            {
                db.Sources.Add(source);
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(e, JsonRequestBehavior.AllowGet);
            }           
        }

    }
}
