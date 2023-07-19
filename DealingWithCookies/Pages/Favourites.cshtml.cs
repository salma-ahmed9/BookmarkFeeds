using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace DealingWithCookies.Pages;

public class FavouritesModel : PageModel
{
    [BindProperty]
    public FavouriteFeed FavouriteFeed { get; set; }
    public List<string> FeedsInfo { get; set; } = new();
    public List<FavouriteFeed> FavouriteFeedList { get; private set; } = new();
    public void OnGet()
    {
        if (!string.IsNullOrEmpty(Request.Cookies["favourites"]))
        {
            FeedsInfo = Request.Cookies["favourites"].Split(',').ToList();
            foreach(var feed in FeedsInfo)
            {
                FavouriteFeed feedObject = new FavouriteFeed();
                feedObject.FeedTitle = feed.Substring(feed.IndexOf('|') + 1);
                feedObject.FeedLink = feed.Substring(0, feed.IndexOf('|'));
                FavouriteFeedList.Add(feedObject);
            }
        }
    }
    /*public async Task<IActionResult> OnPostGetAjaxRequest([FromBody] FavouriteFeed favouriteFeedObject)
    {
        string feedTitle = favouriteFeedObject.FeedTitle;
        string link = favouriteFeedObject.FeedLink;
        string favouriteFeed = Request.Cookies["favourites"];
        string[] feeds = favouriteFeed.Split(',');
        List<string> myFeeds = feeds.ToList();
        List<FavouriteFeed> favouriteFeeds = new List<FavouriteFeed>();
        bool isFavourite = true;
        foreach (var feed in myFeeds)
        {
             FavouriteFeed feedObject = new FavouriteFeed();
             feedObject.FeedTitle = feed.Substring(feed.IndexOf('|') + 1);
             feedObject.FeedLink = feed.Substring(0, feed.IndexOf('|'));
             favouriteFeeds.Add(feedObject);
        }
        FavouriteFeedList = favouriteFeeds;
        foreach(var feed in feeds)
        {
            if (feed == (link+ "|" + feedTitle))
            {
                feeds = feeds.Where(val => val != feed).ToArray();
                foreach (var item in FavouriteFeedList)
                {
                    if (item.FeedTitle == feedTitle && item.FeedLink == link)
                    {
                        item.Visible = false;
                        isFavourite = false;
                        break;
                    }
                }

            }
        }
        favouriteFeed = string.Join(",", feeds);
        Response.Cookies.Append("favourites", favouriteFeed, new CookieOptions
        {
            Secure = Request.IsHttps,
            Path = "/",
            IsEssential = true,
            SameSite = SameSiteMode.None,
            Domain = "localhost",
            HttpOnly = false
        });
        var response = new { message = isFavourite.ToString() };
        return new JsonResult(response);
    }*/
}
public class FavouriteFeed
{
    public string? FeedTitle { get; set; }
    public string? FeedLink { get; set; }
    public bool Visible { get; set; } = true;
}

