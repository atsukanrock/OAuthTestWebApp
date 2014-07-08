using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using NLog;
using OAuthTestWebApp.Logging;
using OAuthTestWebApp.Models.Facebook;

namespace OAuthTestWebApp.Controllers
{
	public class FacebookController : Controller
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.Title = "Facebook";
		}

		[HttpGet]
		public ActionResult Login()
		{
			return View(new LoginViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel viewModel)
		{
			var oathUrl = GetOAuthUrl();

			Logger.Log(LogLevel.Info, HttpContext, "Redirecting to OAuth URL: {0}", oathUrl);

			return Redirect(oathUrl);
		}

		private string GetOAuthUrl()
		{
			var queryStringBuilder = new QueryStringBuilder()
				.Add("client_id", CloudConfigurationManager.GetSetting("Facebook.ApplicationId"))
				.Add("redirect_uri", GetOAuthCallbackUrl())
				.Add("scope", CloudConfigurationManager.GetSetting("Facebook.Scope"));

			return string.Format("https://www.facebook.com/dialog/oauth?{0}", queryStringBuilder.ToQueryString());
		}

		private string GetOAuthCallbackUrl()
		{
			return GetAbsoluteActionUrl("OAuthCallback");
		}

		[HttpGet]
		public async Task<ActionResult> OAuthCallback(string code)
		{
			// Facebook's documentation is poor. The key might not be documented.
			if (string.IsNullOrWhiteSpace(code) ||
				Request.QueryString.AllKeys.Any(key => key.ToLowerInvariant().Contains("error")))
			{
				Logger.Log(LogLevel.Info, HttpContext, "OAuth error occurred.");

				return View(new OAuthCallbackViewModel {Success = false, Description = "Failed."});
			}

			Logger.Log(LogLevel.Info, HttpContext, "OAuth succeeded.");

			var accessTokenUrl = GetAccessTokenUrl(code);

			Logger.Log(LogLevel.Info, HttpContext, "To get access token, requesting to: {0}", accessTokenUrl);

			var response = await new HttpClient().GetAsync(accessTokenUrl);
			var content = await response.Content.ReadAsStringAsync();

			Logger.Log(LogLevel.Info, HttpContext, "The response of getting access token is status code: {0}, content: {1}",
					   response.StatusCode, content);

			var result = HttpUtility.ParseQueryString(content);
			var viewModel = new OAuthCallbackViewModel
			{
				Success = true,
				Description = "Succeeded.",
				AccessToken = result["access_token"],
				SecondsTilExpiration = int.Parse(result["expires"])
			};

			Logger.Log(LogLevel.Info, HttpContext, "Got access token: {0}", viewModel.AccessToken);

			return View(viewModel);
		}

		private string GetAccessTokenUrl(string code)
		{
			// redirect_uri must be the same as the original request_uri that was used when starting the OAuth login process.
			var queryStringBuilder = new QueryStringBuilder()
				.Add("client_id", CloudConfigurationManager.GetSetting("Facebook.ApplicationId"))
				.Add("redirect_uri", GetOAuthCallbackUrl())
				.Add("client_secret", CloudConfigurationManager.GetSetting("Facebook.ApplicationSecret"))
				.Add("code", code);

			return string.Format("https://graph.facebook.com/oauth/access_token?{0}", queryStringBuilder.ToQueryString());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> OAuthCallback(OAuthCallbackViewModel viewModel)
		{
			var queryStringBuilder = new QueryStringBuilder()
				.Add("fields", CloudConfigurationManager.GetSetting("Facebook.Fields"))
				.Add("access_token", viewModel.AccessToken);
			var uri = "https://graph.facebook.com/me?" + queryStringBuilder.ToQueryString();

			Logger.Log(LogLevel.Info, HttpContext, "To get profile, requesting to: {0}", uri);

			var response = await new HttpClient().GetAsync(uri);
			var content = await response.Content.ReadAsStringAsync();

			Logger.Log(LogLevel.Info, HttpContext, "The response of getting profile is status code: {0}, content: {1}",
					   response.StatusCode, content);

			TempData["StatusCode"] = response.StatusCode;
			TempData["Content"] = content;
			return RedirectToAction("MyProfile");
		}

		[HttpGet]
		public ActionResult MyProfile()
		{
			var viewModel = new MyProfileViewModel
			{
				StatusCode = (HttpStatusCode)TempData["StatusCode"],
				Content = (string)TempData["Content"]
			};
			return View(viewModel);
		}

		private string GetAbsoluteActionUrl(string actionName)
		{
			var currentUrl = Request.Url;
			Debug.Assert(currentUrl != null, "currentUrl != null");
			Debug.WriteLine("Request.Url: {0}", currentUrl);
#if DEBUG
			foreach (var uriPartial in Enum.GetValues(typeof(UriPartial)).Cast<UriPartial>())
			{
				Debug.WriteLine("{0}: {1}", uriPartial, currentUrl.GetLeftPart(uriPartial));
			}
#endif
			var actionUrl = Url.Action(actionName);
			var uriBuilder = new UriBuilder(currentUrl.Scheme, currentUrl.Host, currentUrl.Port, actionUrl);
			return uriBuilder.ToString();
		}
	}
}