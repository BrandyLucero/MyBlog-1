using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO; //import namespace for using Path
using System.Web.Security; //import namespace for memberships

namespace MyBlog.Controllers
{
    public class AccountController : Controller
    {
        //set up connection to the db
        Models.MyBlogEntities db = new Models.MyBlogEntities();


        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        //add httpPostedFileBase parameter
        [HttpPost]
        public ActionResult Register(Models.Registration registration, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                //save the image to database
                //GUID generates random characters that we can use to make sure filename is not repeated
                string filename = Guid.NewGuid().ToString().Substring(0, 6) + Image.FileName;
                //specify the path to save the image to (file directory)
                //Server.MapPath gets the physical location of of website on the server
                string path = Path.Combine(Server.MapPath("~/content/"), filename);
                //save the file
                Image.SaveAs(path);
                //update the registration object with the new image
                registration.Image = "/content/" + filename;
            }

            //create a new Membership for user
            Membership.CreateUser(registration.Username, registration.Password);

            //create and populate the new Author object
            Models.Author author = new Models.Author();
            author.Name = registration.Name;
            author.ImageUrl = registration.Image;
            author.Username = registration.Username;
            //add new author into database
            db.Authors.Add(author);
            //save changes
            db.SaveChanges();

            //registration complete, now log in the user
            FormsAuthentication.SetAuthCookie(registration.Username, false);

            //kick user to the create post page
            return RedirectToAction("Index", "Post");
        }

        public ActionResult Logout()
        {
            //log out the user
            FormsAuthentication.SignOut();
            //kick user to the login screen
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.Login login)
        {
            //check to see if they are a valid user
            if (Membership.ValidateUser(login.Username, login.Password))
            {
                //if both entries are valid then log them in
                FormsAuthentication.SetAuthCookie(login.Username, false);
                //kick user to create post page
                return RedirectToAction("Index","Post");
            }
            else
	        {
                //either they entered invalid password or username
                //show error message
                ViewBag.ErrorMessage = "Invalid username and/or password.";
                return View(login);
	        }
        }
    }
}
