using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ToyShopParser.Entities;

namespace ToyShopParser.Parser
{
    public static class ToyParser
    {
        public static int Page { get; private set; }

        public static async Task<ICollection<Toy>> ParseSectionAsync(string address, string city, int delayBtwPages, int delayBtwToys)
        {
            Page = 1;

            var context = CreateContext(city);
            var document = await context.OpenAsync(address);

            var lastPage = document.QuerySelector(".pagination li:nth-last-child(2)").TextContent.Trim();
            int pageCount = 1;

            var toysCountElement = document.QuerySelector("#count_search_results").GetAttribute("value");
            int toysCount = 0;
            int.TryParse(toysCountElement, out toysCount);

            if (lastPage != null)
                int.TryParse(lastPage, out pageCount);

            List<Toy> toys = new List<Toy>(toysCount);
            string[] pagesLinks = new string[pageCount];

            List<Task<ICollection<Toy>>> tasks = new List<Task<ICollection<Toy>>>(pageCount);

            for (int i = 0; i < pageCount; i++)
            {
                pagesLinks[i] = address + $"?PAGEN_8={i + 1}";
            }

            foreach (var page in pagesLinks)
            {
                tasks.Add(Task.Run(() => ParsePageAsync(page, city, delayBtwToys)));
                Task.Delay(delayBtwPages).Wait();
            }

            await Task.WhenAll(tasks);

            for (int i = 0; i < pageCount; i++)
            {
                toys.AddRange(tasks[i].Result);
            }

            return toys;
        }

        public static async Task<ICollection<Toy>> ParsePageAsync(string address, string city, int delayBtwToys, IBrowsingContext context = null)
        {
            if (context == null)
            {
                context = CreateContext(city);
            }

            var document = await context.OpenAsync(address);

            var toysLinks = document.QuerySelectorAll("a.product-name")
                .Select(i => document.Origin + i.GetAttribute("href"))
                .ToArray();

            var toys = new Toy[toysLinks.Length];
            List<Task<Toy>> tasks = new List<Task<Toy>>(toysLinks.Length);

            foreach (var toy in toysLinks)
            {
                tasks.Add(Task.Run(() => ParseToyAsync(toy, city)));
                Task.Delay(delayBtwToys).Wait();
            }

            await Task.WhenAll(tasks);

            for (int i = 0; i < toys.Length; i++)
            {
                toys[i] = tasks[i].Result;
            }

            Console.WriteLine($"{Page} page parsed.");
            Page++;

            return toys;
        }

        public static async Task<Toy> ParseToyAsync(string address, string city, IBrowsingContext context = null)
        {
            if(context == null)
            {
                context = CreateContext(city);
            }

            var document = await context.OpenAsync(address);

            var region = GetRegion(document);
            var name = GetName(document);
            string[] breadcrumbs = GetBreadcrumbs(document);
            var price = GetPrice(document);
            var oldPrice = GetOldPrice(document);
            var isAvailable = GetAvailability(document);
            var pics = GetPics(document);

            return new Toy
            {
                Region = region,
                Breadcrumbs = breadcrumbs,
                Name = name,
                Price = price,
                OldPrice = oldPrice,
                IsAvailable = isAvailable,
                Link = address,
                PicLinks = pics
            };

        }

        private static IBrowsingContext CreateContext(string city)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            handler.CookieContainer.Add(new Cookie("BITRIX_SM_city", city, null, ".www.toy.ru"));

            var client = new HttpClient(handler);

            var requester = new HttpClientRequester(client);

            var config = Configuration.Default
              .With(requester)
              .WithDefaultLoader()
              .WithTemporaryCookies();

            return BrowsingContext.New(config);
        }

        private static string GetRegion(IDocument document)
        {
            return document.QuerySelector("[data-src=\"#region\"]").TextContent.Trim();
        }

        private static string GetName(IDocument document)
        {
            return document.QuerySelector("h1").TextContent;
        }

        private static string[] GetBreadcrumbs(IDocument document)
        {
            return document
                .QuerySelectorAll("a.breadcrumb-item")
                .Select(i => i.GetAttribute("title"))
                .Append(document.QuerySelector("h1").TextContent)
                .ToArray();

        }

        private static uint GetPrice(IDocument document)
        {
            var price = document.QuerySelector(".price")?.TextContent;

            price = price?
                .Substring(0, price.IndexOf('р'))
                .Replace(" ", "");

            uint parsedPrice = 0;
            uint.TryParse(price, out parsedPrice);

            return parsedPrice;
        }

        private static uint GetOldPrice(IDocument document)
        {
            var oldPrice = document.QuerySelector(".old-price")?.TextContent;
            uint parsedOldPrice = 0;

            if (!string.IsNullOrEmpty(oldPrice))
            {
                oldPrice = oldPrice
                    .Substring(0, oldPrice.IndexOf('р'))
                    .Replace(" ", "");

                uint.TryParse(oldPrice, out parsedOldPrice);
            }

            return parsedOldPrice;
        }

        private static bool GetAvailability(IDocument document)
        {
            return document.QuerySelector(".v") != null;
        }

        private static string[] GetPics(IDocument document)
        {
            return document.QuerySelectorAll("a[data-fancybox=\"group\"]>.img-fluid")
                .Select(i => i.GetAttribute("src"))
                .ToArray();
        }
    }
}
