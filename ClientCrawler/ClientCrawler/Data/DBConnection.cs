﻿using ClientCrawler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ClientCrawler.Data
{
    public class DBConnection : DbContext
    {
        public DBConnection() : base("name=CrawDB")
        {
        }

        public DbSet<Source> Sources { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Banner> Banners { get; set; }
    }
}