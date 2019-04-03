using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsApplication.Library.Database;
using NewsApplication.Models;

namespace NewsApplication.Controllers
{
    public class UserManageController : Controller
    {
        public ActionResult Index(int page = 1)
        {
            IDatabaseUtility connection = new MySQLUtility();

            try
            {
                connection.Connect();

            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e;
                return View("_errors");
            }


            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.ADMIN))
                {
                    UserListModel list = new UserListModel(connection);

                    ViewBag.users = list.GetAll((page - 1) * 10, 10);
                    ViewBag.pagepartition = new PagePartitionModel(page, (int)Math.Ceiling(list.GetTotal() / 10.0));

                    return View();
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền thực hiện thao tác này";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }
        }
        [HttpGet]
        public ActionResult GrantRole()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Delete()
        {
            return View();
        }


	}
}