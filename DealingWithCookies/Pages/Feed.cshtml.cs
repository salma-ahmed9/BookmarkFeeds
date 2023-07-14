using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace DealingWithCookies.Pages;

public class FeedModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
    public FeedModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    public List<RssItem> RssItemsList { get; private set; } = new();
    public async Task<List<RssItem>> FetchParse(string feedLink)
    {
        List<RssItem> rssList = new List<RssItem>();
        HttpClient httpClient = _clientFactory.CreateClient();
        HttpResponseMessage httpResponse = await httpClient.GetAsync(feedLink);
        if (httpResponse.IsSuccessStatusCode)
        {
            HttpContent responseContent = httpResponse.Content;
            string responseData = await responseContent.ReadAsStringAsync();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseData);
            XmlNodeList? nodes = doc.DocumentElement?.SelectNodes("//item");
            foreach (XmlNode node in nodes)
            {
                RssItem rssObject = new RssItem();

                string description = (node.SelectSingleNode("description") == null) ? "" : node.SelectSingleNode("description").InnerText;
                HtmlString htmlDescription = new HtmlString(description);
                rssObject.Description = htmlDescription;
                rssObject.Title = (node.SelectSingleNode("title") == null) ? "" : node.SelectSingleNode("title").InnerText;
                rssObject.Link = (node.SelectSingleNode("link") == null) ? "" : node.SelectSingleNode("link").InnerText;
                rssObject.PublishDate = (node.SelectSingleNode("pubDate") == null) ? "" : node.SelectSingleNode("pubDate").InnerText;
                rssList.Add(rssObject);
            }
        }
        return rssList;
    }
    public async Task<IActionResult> OnGetAsync()
    {
        string feedLink = Request.Query["message"];
        var myRssList = await FetchParse(feedLink);
        RssItemsList = myRssList;
        return Page();
    }
}
public class RssItem
{
    public string? Title { get; set; }
    public HtmlString? Description { get; set; }
    public string? Link { get; set; }
    public string? PublishDate { get; set; }
}
