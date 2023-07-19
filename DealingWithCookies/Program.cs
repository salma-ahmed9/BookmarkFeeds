using DealingWithCookies.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
var app = builder.Build();
var services = new ServiceCollection();
services.AddSingleton<FavouritesModel>();
var serviceProvider = services.BuildServiceProvider();
// Retrieve an instance of FavouritesModel
var favouritesModel = serviceProvider.GetService<FavouritesModel>();
var favouriteFeedList = favouritesModel.FavouriteFeedList;

app.MapPost("/fav", async ([FromBody] RssFeed feed, HttpContext context) => {
    string feedTitle = feed.FeedTitle;
    string link = feed.FeedLink;
    string favouriteFeed = context.Request.Cookies["favourites"];
    bool indicator = false;
    if (!string.IsNullOrEmpty(favouriteFeed))
    {
        string[] feeds = favouriteFeed.Split(',');
        for (int i = 0; i < feeds.Length; i++)
        {
            if (feeds[i] == (link + "|" + feedTitle))
            {
                feeds = feeds.Where((val, idx) => idx != i).ToArray();
                indicator = true;
                break;
            }
        }
        favouriteFeed = string.Join(",", feeds);
        if (!indicator)
        {
            favouriteFeed += "," + link + "|" + feedTitle;
        }
    }
    else
    {
        favouriteFeed = link + "|" + feedTitle;
    }
    context.Response.Cookies.Append("favourites", favouriteFeed, new CookieOptions
    {
        Secure = context.Request.IsHttps,
        Path = "/",
        IsEssential = true,
        SameSite = SameSiteMode.None,
        Domain = "localhost",
        HttpOnly = false
    });
    return new { message = indicator.ToString() };
});

app.MapPost("/Unfav", async ([FromBody] RssFeed feed, HttpContext context) => {
    string feedTitle = feed.FeedTitle;
    string link = feed.FeedLink;
    string favouriteFeed = context.Request.Cookies["favourites"];
    string[] feeds = favouriteFeed.Split(',');
    List<string> myFeeds = feeds.ToList();
    List<FavouriteFeed> favouriteFeeds = new List<FavouriteFeed>();
    bool isFavourite = true;
    foreach (var myfeed in myFeeds)
    {
        FavouriteFeed feedObject = new FavouriteFeed();
        feedObject.FeedTitle = myfeed.Substring(myfeed.IndexOf('|') + 1);
        feedObject.FeedLink = myfeed.Substring(0, myfeed.IndexOf('|'));
        favouriteFeeds.Add(feedObject);
    }
    favouriteFeedList = favouriteFeeds;
    foreach (var myfeed in feeds)
    {
        if (myfeed == (link + "|" + feedTitle))
        {
            feeds = feeds.Where(val => val != myfeed).ToArray();
            foreach (var item in favouriteFeedList)
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
    context.Response.Cookies.Append("favourites", favouriteFeed, new CookieOptions
    {
        Secure = context.Request.IsHttps,
        Path = "/",
        IsEssential = true,
        SameSite = SameSiteMode.None,
        Domain = "localhost",
        HttpOnly = false
    });
    var response = new { message = isFavourite.ToString() };
    return new JsonResult(response);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();