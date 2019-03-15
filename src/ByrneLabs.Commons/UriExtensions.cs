using System;
using System.Linq;
using System.Text;

namespace ByrneLabs.Commons
{
    public static class UriExtensions
    {
        public static Uri AddQueryParameter(this Uri uri, string queryParameterName, string queryParameterValue)
        {
            var queryBuilder = new StringBuilder(uri.Query);
            if (string.IsNullOrWhiteSpace(uri.Query))
            {
                queryBuilder.Append("?");
            }
            else if (uri.Query.Length > 1)
            {
                queryBuilder.Append("&");
            }

            queryBuilder.Append(queryParameterName).Append("=").Append(queryParameterValue);

            var uriBuilder = new UriBuilder(uri);
            uriBuilder.Query = queryBuilder.ToString();

            return uriBuilder.Uri;
        }

        public static Uri RemoveQueryParameter(this Uri uri, string queryParameterName)
        {
            if (string.IsNullOrWhiteSpace(uri.Query))
            {
                return uri;
            }

            var uriBuilder = new UriBuilder(uri);
            var queryParameters = uriBuilder.Query.SubstringAfterLast("?").Split('&').ToList();
            var queryParametersToRemove = queryParameters.Where(qp => qp.StartsWith(queryParameterName + "=", StringComparison.Ordinal)).ToArray();
            foreach (var queryParameterToRemove in queryParametersToRemove)
            {
                queryParameters.Remove(queryParameterToRemove);
            }

            uriBuilder.Query = queryParameters.Any() ? "?" + queryParameters.Join("&") : string.Empty;

            return uriBuilder.Uri;
        }
    }
}
