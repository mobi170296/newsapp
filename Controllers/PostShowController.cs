using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsApplication.Models;
using NewsApplication.Library.Database;
using System.Data;

namespace NewsApplication.Controllers
{
    public class PostShowController : Controller
    {
        public ActionResult ListByCategory(string category, int page = 1)
        {

            MySQLUtility connection = new MySQLUtility();
            try
            {
                connection.Connect();
                using(IDataReader result = connection.select("*").from("category").where("link=" + new DBString(category).SqlValue()).Execute()){
                    if (result.Read())
                    {
                        ViewBag.category = new Category();
                        ViewBag.category.id = (int)result["id"];
                        ViewBag.category.name = (string)result["name"];
                        ViewBag.category.link = (string)result["link"];
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Danh mục này không tồn tại!";
                        return View("_errors");
                    }
                }
                PostListModel list = new PostListModel(connection);
                List<Post> posts = list.GetByCategoryLimit(category, page, 20, "created_time desc");
                PagePartitionModel pagepartition = new PagePartitionModel("ListByCategory", "PostShow", page, (int)Math.Ceiling(list.GetTotalPageByCategory(category)/20.0));
                ViewBag.pagepartition = pagepartition;
                return View(posts);
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }
            finally
            {
                connection.Close();
            }
        }
	}
}