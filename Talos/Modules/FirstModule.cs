using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace TalosBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("pong");
        }
        [Command("double")]
        public async Task Double(long num)
        {
            await ReplyAsync("You entered: " + num + ". Your number doubled equals " + num*2 + ".");
        }
        public class UrlResult
        {
            public string url { get; set; }
        }
        [Command("cat")]
        public async Task catPosting()
        {
            Random rng = new Random();
            var searchtext = "cat-" + (char) (65 + rng.Next(25));
            var url = "https://tenor.googleapis.com/v2/search?q=" + searchtext + "&key=" + File.ReadAllText(@"C:\TalosFiles\tenortoken.txt") + "&client_key=talosbot&limit=10&media_filter=gif,tinygif";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var fetch = await response.Content.ReadAsStringAsync();
                    JObject search = JObject.Parse(fetch);
                    IList<JToken> results = search["results"].Children().ToList();
                    IList<UrlResult> urlResults = new List<UrlResult>();
                    foreach (JToken result in results)
                    {
                        UrlResult urlResult = result.ToObject<UrlResult>();
                        urlResults.Add(urlResult);
                    }
                    await ReplyAsync(urlResults[rng.Next(urlResults.Count)].url);
                    
                    
                }
                else await ReplyAsync("Sorry! An error has occured. Please try again.");
            }
        }
    }
}