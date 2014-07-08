using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAuthTestWebApp
{
	public class QueryStringBuilder
	{
		private readonly List<Tuple<string, string>> _items = new List<Tuple<string, string>>();

		public QueryStringBuilder Add(string name, string value)
		{
			_items.Add(Tuple.Create(name, value));
			return this;
		}

		public string ToQueryString()
		{
			var items = _items.Select(item => string.Format("{0}={1}",
															HttpUtility.UrlEncode(item.Item1),
															HttpUtility.UrlEncode(item.Item2)))
							  .ToArray();
			return string.Join("&", items);
		}

		public override string ToString()
		{
			return ToQueryString();
		}
	}
}