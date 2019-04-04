using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsApplication.Models;
using NewsApplication.Library.Database;
using MySql.Data.MySqlClient;
using NewsApplication.Exception;
using System.Security.Cryptography;
using System.Text;

namespace NewsApplication.Controllers
{
    public class PostManageController : Controller
    {
        public ActionResult Index(int page = 1)
        {
            IDatabaseUtility connection = new MySQLUtility();

            try
            {
                connection.Connect();
            }catch(DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }


            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.JOURNALIST))
                {

                    PostListModel list = new PostListModel(connection);
                    ViewBag.posts = list.GetWhere("journalist_id=" + user.id, (page - 1) * 10, 10);
                    int total = list.GetTotalWhere("journalist_id=" + user.id);
                    ViewBag.pagepartition = new PagePartitionModel("Index", "PostManage", page, (int)Math.Ceiling(total /10.0));
                    return View();
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập";
                    return View("_errors");
                }
            }catch(DBException e)
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
        public ActionResult Add()
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

                if(user.IsLogin() && user.HaveRole(NewsApplication.Models.User.JOURNALIST))
                {
                    List<int> ids = new List<int>();
                    using(MySqlDataReader result = (MySqlDataReader)connection.select("*").from("category").Execute())
                    {
                        while(result.Read())
                            ids.Add(result.GetInt32("id"));
                    }

                    ViewBag.categories = new List<Category>();

                    foreach(int id in ids)
                    {
                        Category cate = new Category(connection);
                        cate.id = id;
                        cate.Load();
                        ViewBag.categories.Add(cate);
                    }
                    return View();
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập vào đây";
                    return View("_errors");
                }
            }catch(DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View();
            }
            finally
            {
                connection.Close();
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Add(Post post, HttpPostedFileBase poster)
        {
            IDatabaseUtility connection = new MySQLUtility();

            try
            {
                connection.Connect();
            }catch(DBException e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("_errors");
            }



            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.JOURNALIST))
                {
                    if(poster == null)
                    {
                        post.AddErrorMessage("poster", "Bạn chưa chọn ảnh bìa cho tin tức");
                    }
                    post.SetConnection(connection);
                    ViewBag.categories = new CategoryListModel(connection).GetAll();

                    post.CheckValidForSummary().CheckValidForContent().CheckValidForTitle().CheckValidForCategoryId();

                    if(post.GetErrorsMap().Count() == 0)
                    {
                        PostImage image = new PostImage(connection);
                        post.valid = 0;
                        post.journalist_id = user.id;
                        post.Add();
                        image.post_id = (int)connection.GetLastInsertedId();
                        image.path = PostImage.POSTER_IMAGE_DIR + image.post_id + "_" + new Random().Next() + System.IO.Path.GetExtension(poster.FileName);
                        image.Add();

                        poster.SaveAs(Server.MapPath(image.path));
                        TempData["SuccessMessage"] = "Bạn đã đăng bài thành công hãy tìm nhà kiểm duyệt để duyệt bài của bạn và hiển thị nó";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.postback = post;
                        throw new InputException(1, post.GetErrorsMap());
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không thể truy cập trang web này";
                    return View("_errors");
                }
            }catch(DBException e)
            {
                ViewBag.ErrorMessage = "" + e.Message;
                return View("_errors");
            }catch(InputException e)
            {
                ViewBag.ErrorsMap = e.Errors;
                return View();
            }
            finally
            {
                connection.Close();
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(HttpPostedFileBase poster, Post newdata)
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



            Post post = new Post(connection);
            try
            {
                Authenticate authenticate = new Authenticate(connection);
                User user = authenticate.GetUser();
                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.JOURNALIST))
                {
                    ViewBag.categories = new CategoryListModel(connection).GetAll();
                    newdata.SetConnection(connection);
                    post.id = newdata.id;
                    if (post.LoadPost())
                    {
                        post.LoadPoster();
                        if (post.journalist_id == user.id)
                        {
                            newdata.CheckValidForCategoryId().CheckValidForContent().CheckValidForSummary().CheckValidForTitle();

                            if (newdata.GetErrorsMap().Count == 0)
                            {
                                if (post.Update(newdata))
                                {
                                    if (poster != null)
                                    {
                                        string filename = post.poster.path;
                                        post.poster.SetConnection(connection);
                                        if (System.IO.File.Exists(Server.MapPath(filename)))
                                        {
                                            System.IO.File.Delete(Server.MapPath(filename));
                                        }

                                        try
                                        {
                                            PostImage image = new PostImage(connection);
                                            image.post_id = post.id;
                                            image.path = PostImage.POSTER_IMAGE_DIR + image.post_id + "_" + new Random().Next() + System.IO.Path.GetExtension(poster.FileName);

                                            post.poster.Update(image);
                                            poster.SaveAs(Server.MapPath(image.path));
                                        }
                                        catch(System.Exception e)
                                        {
                                            ViewBag.ErrorMessage = e.Message;
                                            ViewBag.oldtitle = post.title;
                                            newdata.poster = post.poster;
                                            return View(newdata);
                                        }
                                    }
                                    TempData.Add("SuccessMessage", "Bạn đã cập nhật tin tức thành công!");
                                    return RedirectToAction("Index", "PostManage");
                                }
                                else
                                {
                                    ViewBag.ErrorMessage = "Cập nhật tin tức không thành công!";
                                    ViewBag.oldtitle = post.title;
                                    newdata.poster = post.poster;
                                    return View(newdata);
                                }
                            }
                            else
                            {
                                throw new InputException(1, newdata.GetErrorsMap());
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Tin tức này không thuộc quyền sở hữu của bạn";
                            return View("_errors");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Tin tức này không tồn tại!";
                        return View("_errors");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập vào đây";
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
                ViewBag.oldtitle = post.title;
                newdata.poster = post.poster;
                return View(newdata);
            }
            finally
            {
                connection.Close();
            }
        }

        [HttpGet]
        public ActionResult Update(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Trang này không tồn tại!";
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

                if (user.IsLogin() && user.HaveRole(NewsApplication.Models.User.JOURNALIST))
                {
                    ViewBag.categories = new CategoryListModel(connection).GetAll();
                    Post post = new Post(connection);
                    post.id = (int)id;
                    if (post.LoadPost())
                    {
                        post.LoadInspector();
                        post.LoadCategory();
                        post.LoadJournalist();
                        post.LoadPoster();
                        ViewBag.oldtitle = post.title;
                        return View(post);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Bài viết này không tồn tại hoặc không phải thuộc quyền sở hữu của bạn";
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