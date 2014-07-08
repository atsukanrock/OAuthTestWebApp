using System;
using System.Web;
using NLog;

namespace OAuthTestWebApp.Logging
{
	public static class LoggerExtensions
	{
		public static void Log(this Logger logger, LogLevel logLevel, HttpContextBase httpContext, string message,
							   params object[] args)
		{
			var logEventInfo = new LogEventInfo
			{
				Level = logLevel,
				Message = message,
				Parameters = args,
				TimeStamp = DateTime.Now
			};

			if (httpContext != null)
			{
				var req = httpContext.Request;
				logEventInfo.Properties["host"] = req.UserHostAddress;
				logEventInfo.Properties["referer"] = req.UrlReferrer;
				logEventInfo.Properties["ua"] = req.UserAgent;
				logEventInfo.Properties["uri"] = req.Url;
			}

			logger.Log(logEventInfo);
		}
	}
}