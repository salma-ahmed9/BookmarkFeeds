using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace DealingWithCookies.Pages;

public class FavouritesModel : PageModel
{
    public List<string> FeedsInfo { get; set; } = new();
    public List<FavouriteFeed> FavouriteFeedList { get; private set; } = new();
    public void OnGet()
    {
        if (Request.Cookies["favouriteFeeds"] is not null)
        {
            FeedsInfo = Request.Cookies["favouriteFeeds"].Split(',').ToList();
            foreach(var feed in FeedsInfo)
            {
                FavouriteFeed feedObject = new FavouriteFeed();
                feedObject.FeedTitle = feed.Substring(feed.IndexOf('|') + 1);
                feedObject.FeedLink= feed.Substring(0,feed.IndexOf('|'));
                FavouriteFeedList.Add(feedObject);
            }
        }
    }
    public async Task<IActionResult> OnPost()
    {
        string feedLink = Request.Form["feedLink"];
        string feedTitle = Request.Form["feedTitle"];
        string favouriteFeed = Request.Cookies["favouriteFeeds"];
        string[] feeds = favouriteFeed.Split(',');
        List<string> myFeeds = feeds.ToList();
        List<FavouriteFeed> favouriteFeeds = new List<FavouriteFeed>();
        foreach (var feed in myFeeds)
        {
            FavouriteFeed feedObject = new FavouriteFeed();
            feedObject.FeedTitle = feed.Substring(feed.IndexOf('|') + 1);
            feedObject.FeedLink = feed.Substring(0, feed.IndexOf('|'));
            favouriteFeeds.Add(feedObject);
        }
        FavouriteFeedList = favouriteFeeds;
        for (int i = 0; i < feeds.Length; i++)
        {
            if (feeds[i] == (feedLink + "|" + feedTitle))
            {
                feeds = feeds.Where((val, idx) => idx != i).ToArray();
                foreach(var item in FavouriteFeedList)
                {
                    if(item.FeedTitle==feedTitle && item.FeedLink==feedLink)
                    {
                        item.Visible = false;
                        break;
                    }
                }
                
            }
        }
        favouriteFeed = string.Join(",", feeds);
        Response.Cookies.Append("favouriteFeeds", favouriteFeed, new CookieOptions
        {
            Secure = Request.IsHttps,
            Path = "/",
            IsEssential = true,
            SameSite = SameSiteMode.None,
            Domain = "localhost",
            HttpOnly = false
        });
        return RedirectToPage("/Favourites");
    }
}
public class FavouriteFeed
{
    public string? FeedTitle { get; set; }
    public string? FeedLink { get; set; }
    public bool Visible { get; set; } = true;
}

