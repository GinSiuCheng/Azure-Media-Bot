using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Web;
using System.Web.UI.WebControls;

namespace videoBot.Models
{
    public class VideoMapper 
    {
        public static SearchHit ToSearchHit(SearchResult hit)
        {
            // Create search results object 
            var searchHit = new SearchHit();

            // Video Indexer API 
            var apiUrl = "https://api.videoindexer.ai";
            var accountId = ConfigurationManager.AppSettings["VideoIndexerAccount"];
            string videoIndexerKey = ConfigurationManager.AppSettings["VideoIndexerKey"];
            var location = "trial";

            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var viClient = new HttpClient(handler);
            viClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", videoIndexerKey);

            var videoTokenRequestResult = viClient.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/Videos/{(string)hit.Document["id"]}/AccessToken?allowEdit=true").Result;
            var videoAccessToken = videoTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");
            viClient.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key"); 

            var videoGetIndexRequestResult = viClient.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{(string)hit.Document["id"]}/Index?accessToken={videoAccessToken}&language=English").Result;
            var videoGetIndexResult = videoGetIndexRequestResult.Content.ReadAsStringAsync().Result;
            var videoIndexData = JsonConvert.DeserializeObject<dynamic>(videoGetIndexResult); 

            var playerWidgetRequestResult = viClient.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{(string)hit.Document["id"]}/PlayerWidget?accessToken={videoAccessToken}").Result;
            var playerWidgetLink = playerWidgetRequestResult.Headers.Location;
            
            searchHit.Key = (string)hit.Document["id"];
            searchHit.PictureUrl = $"https://www.videoindexer.ai/api/v2/accounts/{accountId}/videos/{(string)hit.Document["id"]}/thumbnails/{videoIndexData["summarizedInsights"]["thumbnailId"]}?accessToken={videoAccessToken}";
            searchHit.Title = (string)hit.Document["name"];
            searchHit.Description = playerWidgetLink.ToString();

            return searchHit;
        }
    }
}