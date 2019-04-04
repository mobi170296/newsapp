using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using System.Collections;
using NewsApplication.Library.Database;
using NewsApplication.Models;

namespace NewsApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IDatabaseUtility connection = new MySQLUtility();

            try
            {
                connection.Connect();
                CategoryListModel categorylist = new CategoryListModel(connection);

                List<Category> categories = categorylist.GetAll();

                List<List<Post>> categoryposts = new List<List<Post>>();

                foreach (Category category in categories)
                {
                    PostListModel postlist = new PostListModel(connection);
                    List<Post> posts = postlist.GetByCategoryLimit(category.link, 0, 5, "created_time DESC");
                    categoryposts.Add(posts);
                }

                ViewBag.categories = categories;
                return View(categoryposts);
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }
        }
    }
}