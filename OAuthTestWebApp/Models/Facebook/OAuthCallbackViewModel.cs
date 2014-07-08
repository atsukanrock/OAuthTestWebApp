using System;

namespace OAuthTestWebApp.Models.Facebook
{
	public class OAuthCallbackViewModel
	{
		public bool Success { get; set; }

		public string Description { get; set; }

		public string AccessToken { get; set; }

		public int SecondsTilExpiration { get; set; }

		public DateTimeOffset ExpiresApproximatelyAt
		{
			get { return DateTimeOffset.UtcNow.AddSeconds(SecondsTilExpiration); }
		}
	}
}