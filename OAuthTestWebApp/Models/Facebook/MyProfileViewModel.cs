using System.Net;

namespace OAuthTestWebApp.Models.Facebook
{
	public class MyProfileViewModel
	{
		public HttpStatusCode StatusCode { get; set; }

		public string Content { get; set; }
	}
}