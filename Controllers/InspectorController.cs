using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsApplication.Models;
using NewsApplication.Library.Database;

namespace NewsApplication.Controllers
{
    public class InspectorController : Controller
    {
        // GET: Inspector
        public ActionResult Index()
        {

            return RedirectToAction("NoApprovedList");
        }

        public ActionResult NoApprovedList(int page = 1)
        {
            IDatabaseUtility connection = new MySQLUtility();

            try
            {
                connection.Connect();
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = "" + e;
                return View("_errors");
            }

            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();
                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.INSPECTOR))
                {
                    PostListModel list = new PostListModel(connection);
                    ViewBag.posts = list.GetWhere("valid=0", (page - 1) * 10, 10);
                    ViewBag.pagepartition = new PagePartitionModel("NoApprovedList", "Inspector", page, (int)Math.Ceiling(list.GetTotalWhere("valid=0") / 10.0));
                    return View();
                }else{
                    ViewBag.ErrorMessage = "Bạn không thể truy cập trang này";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = "" + e;
                return View("_errors");
            }
        }
        public ActionResult ApprovedList(int page = 1)
        {
            IDatabaseUtility connection = new MySQLUtility();

            try
            {
                connection.Connect();
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = "" + e;
                return View("_errors");
            }

            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.INSPECTOR))
                {
                    PostListModel list = new PostListModel(connection);
                    ViewBag.posts = list.GetWhere("valid=1", (page - 1) * 10, 10);
                    ViewBag.pagepartition = new PagePartitionModel("ApprovedList", "Inspector", page, (int)Math.Ceiling(list.GetTotalWhere("valid=0")/10.0));
                    return View();
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không thể truy cập trang này";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = "" + e;
                return View("_errors");
            }
        }
        public ActionResult ViewToApprove(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Trang này không tồn tại";
                return View("_errors");
            }
            IDatabaseUtility connection = new MySQLUtility();
            try
            {
                connection.Connect();
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }

            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.INSPECTOR))
                {
                    Post post = new Post(connection);
                    post.id = (int)id;
                    if (post.LoadPost())
                    {
                        if (post.IsHidden())
                        {
                            post.LoadCategory();
                            post.LoadInspector();
                            post.LoadJournalist();
                            post.LoadPoster();
                            return View(post);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Trang này không tồn tại";
                            return View("_errors");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Trang này không tồn tại";
                        return View("_errors");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = "" + e.Message;
                return View("_errors");
            }
        }
        public ActionResult Review(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Trang này không tồn tại";
                return View("_errors");
            }
            IDatabaseUtility connection = new MySQLUtility();
            try
            {
                connection.Connect();
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }

            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.INSPECTOR))
                {
                    Post post = new Post(connection);
                    post.id = (int)id;
                    if (post.LoadPost())
                    {
                        if (post.IsShown())
                        {
                            post.LoadCategory();
                            post.LoadInspector();
                            post.LoadJournalist();
                            post.LoadPoster();
                            return View(post);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Trang này không tồn tại";
                            return View("_errors");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Trang này không tồn tại";
                        return View("_errors");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = "" + e.Message;
                return View("_errors");
            }
        }
        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Trang này không tồn tại";
                return View("_errors");
            }

            IDatabaseUtility connection = new MySQLUtility();
            try
            {
                connection.Connect();
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }

            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.INSPECTOR))
                {
                    Post post = new Post(connection);
                    post.id = (int)id;

                    if (post.LoadPost())
                    {
                        post.inspector_id = user.id;
                        post.Approve();
                        TempData["SuccessMessage"] = "Đã duyệt thành công";
                        return RedirectToAction("Index", "Posts", new { id = post.id });
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Trang này không tồn tại";
                        return View("_errors");
                    }
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
    }
}