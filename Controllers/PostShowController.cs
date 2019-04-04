using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsApplication.Models;
using NewsApplication.Library.Database;

namespace NewsApplication.Controllers
{
    public class PostShowController : Controller
    {
        public ActionResult ListByCategory(string category)
        {
            MySQLUtility connection = new MySQLUtility();
            try
            {
                connection.Connect();
                PostListModel list = new PostListModel(connection);
                List<Post> posts = list.GetByCategory(category);
                return View(posts);
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }
        }
	}
}