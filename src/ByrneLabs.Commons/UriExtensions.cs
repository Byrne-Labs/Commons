using System;
using System.Linq;

namespace ByrneLabs.Commons
{
    public static class UriExtensions
    {
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
