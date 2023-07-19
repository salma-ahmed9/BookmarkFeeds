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
}
public class FavouriteFeed
{
    public string? FeedTitle { get; set; }
    public string? FeedLink { get; set; }
    public bool Visible { get; set; } = true;
}

