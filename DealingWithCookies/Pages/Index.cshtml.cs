using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace DealingWithCookies.Pages;
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
    public IndexModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    public List<RssFeed> RssFeedList { get; private set; } = new();
    public int CurrentPage { get; private set; }

    public async Task<List<RssFeed>> FetchParse()
    {
        List<RssFeed> feedList = new List<RssFeed>();
        HttpClient httpClient = _clientFactory.CreateClient();
        HttpResponseMessage httpResponse = await httpClient.GetAsync("https://blue.feedland.org/opml?screenname=dave");
        if (httpResponse.IsSuccessStatusCode)
        {
            HttpContent responseContent = httpResponse.Content;
            string responseData = await responseContent.ReadAsStringAsync();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseData);
            XmlNodeList? nodes = doc.DocumentElement?.GetElementsByTagName("outline");
            string favourites = Request.Cookies["favouriteFeeds"];
            foreach (XmlNode node in nodes)
            {
                RssFeed feedObject = new RssFeed();
                feedObject.FeedTitle = node.Attributes?["text"]?.Value;
                feedObject.FeedLink = node.Attributes?["xmlUrl"]?.Value;
                if(favourites is not null)
                {
                    string[]favourite =  favourites.Split(',');
                    for(int i=0; i<favourite.Length;i++)
                    {
                        if (favourite[i] == (feedObject.FeedLink + "|" + feedObject.FeedTitle))
                        {
                            feedObject.IsFavourite = true;
                            break;
                        }
                    }
                }
                feedList.Add(feedObject);
            }
        }
        httpClient.Dispose();
        return feedList;
    }
    public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int itemsPerPage = 5)
    {
        var myFeedList = await FetchParse();
        int startIndex = (pageNumber - 1) * itemsPerPage;
        int totalItems = myFeedList.Count;
        int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
        if (startIndex >= totalItems)
        {
            startIndex = (totalPages - 1) * itemsPerPage;
            pageNumber = totalPages;
        }
        RssFeedList = myFeedList.GetRange(startIndex, Math.Min(itemsPerPage, totalItems - startIndex));
        CurrentPage = pageNumber;
        ViewData["TotalPages"] = totalPages;
        return Page();
    }

    public async Task<IActionResult> OnPost(int pageNumber = 1, int itemsPerPage = 5)
    {
        string feedLink = Request.Form["feedLink"];
        string feedTitle = Request.Form["feedTitle"];
        string favouriteFeed = Request.Cookies["favouriteFeeds"];
        bool indicator = false;
        if (favouriteFeed is not null)
        {
            string[] feeds = favouriteFeed.Split(',');
            for(int i=0; i<feeds.Length;i++)
            {
                if (feeds[i]==(feedLink + "|" + feedTitle))
                {
                    feeds = feeds.Where((val, idx) => idx != i).ToArray();
                    indicator = true;
                    break;
                }
            }
            favouriteFeed = string.Join(",", feeds);
            if (!indicator)
            {

                favouriteFeed += "," + feedLink + "|" + feedTitle;
                indicator = true;
            }
        }
        else
        {
            favouriteFeed = feedLink + "|" + feedTitle;
        }
       
        Response.Cookies.Append("favouriteFeeds", favouriteFeed, new CookieOptions
        {
            Secure = Request.IsHttps,
            Path = "/",
            IsEssential = true,
            SameSite= SameSiteMode.None,
            Domain = "localhost",
            HttpOnly= false
        });
        var myFeedList = await FetchParse();
        int startIndex = (pageNumber - 1) * itemsPerPage;
        int totalItems = myFeedList.Count;
        int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
        if (startIndex >= totalItems)
        {
            startIndex = (totalPages - 1) * itemsPerPage;
            pageNumber = totalPages;
        }
        RssFeedList = myFeedList.GetRange(startIndex, Math.Min(itemsPerPage, totalItems - startIndex));
        CurrentPage = pageNumber;
        ViewData["TotalPages"] = totalPages;
        return  RedirectToPage("/Index", new { pageNumber = CurrentPage, itemsPerPage=5}); ;
    }
}
public class RssFeed
{
    public string? FeedTitle { get; set; }
    public string? FeedLink { get; set; }
    public bool IsFavourite { get; set; } = false;
}
