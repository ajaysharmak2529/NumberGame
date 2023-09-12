using System.Net;

namespace NumGameWeb
{
    public class RequestHelper
    {
        public static async Task<HttpResponseMessage?> SendHttpRequest(HttpClient httpClient, HttpMethod method, string url, Dictionary<string, string> headers = null, Dictionary<string, string> queryParams = null, string body = null) {
            try {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if (queryParams != null && queryParams.Count > 0)
                {
                    var queryParameters = new List<string>();
                    foreach (var kvp in queryParams)
                    {
                        queryParameters.Add($"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}");
                    }
                    url += "?" + string.Join("&", queryParameters);
                }

                HttpRequestMessage request = new HttpRequestMessage(method, url);

                if (headers != null && headers.Count > 0)
                {
                    foreach (var kvp in headers)
                    {
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

                if (!string.IsNullOrEmpty(body))
                {
                    request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await httpClient.SendAsync(request);
                return response;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
