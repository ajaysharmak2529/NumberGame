using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore
{
    public static class UrlHelperExtensions
    {
        public static string GetLocalUrl(this IUrlHelper urlHelper, string localUrl)
        {
            if (!urlHelper.IsLocalUrl(localUrl))
            {
                return urlHelper!.Page("/Index")!;
            }

            return localUrl;
        }
    }
}
