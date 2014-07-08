using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OAuthTestWebApp.Controllers
{
	public class HomeController : Controller
	{
		//public ActionResult Index()
		//{
		//	return View();
		//}

		public ActionResult About()
		{
			ViewBag.Message = "This application is aimed at testing whether OAuth works on your browser or not.";

			return View();
		}

		//public ActionResult Contact()
		//{
		//	ViewBag.Message = "Your contact page.";

		//	return View();
		//}
	}
}