using ClientCrawler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClientCrawler.Controllers
{
    public class ClientController : Controller
    {
        DBConnection db = new DBConnection();
        // GET: Client
        public ActionResult Index()
        {
            ViewData["category"] = db.Categories.ToList().Take(5);
            ViewData["article_col1"] = db.Categories.ToList().Take(1);
            return View(db.Articles.ToList());
        }
    }
}