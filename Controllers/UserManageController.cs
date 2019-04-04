using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsApplication.Library.Database;
using NewsApplication.Models;
using NewsApplication.Exception;

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
            finally
            {
                connection.Close();
            }
        }
        [HttpGet]
        public ActionResult GrantRole(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Trang web này không tồn tại!";
                return View("_errors");
            }

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
                    User grantUser = new User(connection);
                    grantUser.id = (int)id;
                    if (grantUser.Load())
                    {
                        return View(grantUser);
                    }
                    else
                    {
                        ViewBag.ErrorMessage("Nguời dùng này không tồn tại!");
                        return View("_errors");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập trang này";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e;
                return View("_errors");
            }
            finally
            {
                connection.Close();
            }
        }
        [HttpPost]
        public ActionResult GrantRole(User grantUser)
        {
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


            User destUser = new User(connection);
            destUser.id = grantUser.id;

            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.ADMIN))
                {
                    if (destUser.Load())
                    {
                        if (user.id == destUser.id)
                        {
                            grantUser.AddErrorMessage("invalidid", "Bạn không thể gán quyền cho chính mình được!");
                        }
                        grantUser.CheckValidForRole();
                        if (grantUser.GetErrorsMap().Count != 0)
                        {
                            throw new InputException(1, grantUser.GetErrorsMap());
                        }
                        destUser.GrantRole(grantUser.role);
                        TempData["SuccessMessage"] = "Bạn đã gán quyền cho người dùng " + destUser.username + " thành công!";
                        return RedirectToAction("Index", "UserManage");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Người dùng này không tồn tại!";
                        return View("_errors");
                    }

                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập trang này";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }
            catch (InputException e)
            {
                ViewBag.ErrorsMap = e.Errors;
                return View(destUser);
            }
            finally
            {
                connection.Close();
            }
        }
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Người dùng này không tồn tại!";
                return View("_errors");
            }
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
                    User destUser = new User(connection);
                    destUser.id = (int)id;
                    if (destUser.Load())
                    {
                        return View(destUser);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Người dùng này không tồn tại";
                        return View("_errors");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không thể truy cập trang này";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e;
                return View("_errors");
            }
            finally
            {
                connection.Close();
            }
        }

        [HttpPost]
        public ActionResult Delete(User deleteUser)
        {
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

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.ADMIN))
                {
                    deleteUser.SetConnection(connection);
                    if (deleteUser.Load())
                    {
                        if (deleteUser.id == user.id)
                        {
                            deleteUser.AddErrorMessage("invalidid", "Bạn không thể tự xóa chính mình");
                        }

                        if (deleteUser.GetErrorsMap().Count != 0)
                        {
                            throw new InputException(1, deleteUser.GetErrorsMap());
                        }
                        else
                        {
                            deleteUser.Delete();
                            TempData["ErrorMessage"] = "Bạn đã xóa người dùng " + deleteUser.username;
                            return RedirectToAction("Index", "UserManage");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Người dùng này không tồn tại!";
                        return View("_errors");
                    }
                }else{
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập trang này";
                    return View("_errors");
                }
            }
            catch (DBException e)
            {
                ViewBag.ErrorMessage = e;
                return View("_errors");
            }
            catch (InputException e)
            {
                ViewBag.ErrorsMap = e.Errors;
                return View(deleteUser);
            }
            finally
            {
                connection.Close();
            }
        }
	}
}