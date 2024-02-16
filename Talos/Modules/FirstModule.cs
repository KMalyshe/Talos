using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Security.Cryptography;

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
            string[] searchOptions = ["cat-" + (char) (65 + rng.Next(25)), "this-cat-is-so", "cat-eepy", "cat-caption", "cat", "cat-goofy", "cat-voices", "cat-crazy", "cat-stare", "but-heres-the", "cat-angry",
            "orange-cat", "cat-meme", "cat-punch", "cat-smack"];
            string actualSearch = searchOptions[rng.Next(searchOptions.Length)];
            var url = "https://tenor.googleapis.com/v2/search?q=" + actualSearch + "&key=" + File.ReadAllText(@"C:\TalosFiles\tenortoken.txt") + "&client_key=talosbot&limit=10&media_filter=gif,tinygif";
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
        [Command ("userinfo")]
        [Alias ("whois", "user")]

        public async Task UserInfo (SocketUser user = null)
        {
            var userinfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync("Username: " + userinfo.Username + "\nDate created: " + userinfo.CreatedAt);
        }

        [Command ("roll")]

        public async Task Roll(string dice)
        {
            var diceroll = dice.Split("d");
            var total = 0;
            List<int> numbers = new List<int>();
            if (diceroll.Count() != 2) await ReplyAsync("Invalid Roll. Format: XdY, X and Y being integers.");
            else if (!diceroll[0].All(c => c >= '0' && c <= '9') || !diceroll[1].All(c => c >= '0' && c <= '9')) await ReplyAsync("Invalid Roll. Format: XdY, X and Y being integers.");
            else if ((long.Parse(diceroll[0]) > 2147483647) || (long.Parse(diceroll[0]) > 2147483647)) await ReplyAsync("Rolls too large. Integer limit: 2147483647");
            else if ((long.Parse(diceroll[0]) < 0) || (long.Parse(diceroll[1]) < 0)) await ReplyAsync("Cannot roll negative dice.");
            else
            {
                for (int i = 0; i<Int32.Parse(diceroll[0]); i++)
                {
                    Random rng = new Random();
                    var randomnumber = rng.Next(Int32.Parse(diceroll[1])) + 1;
                    numbers.Add(randomnumber);
                    total += randomnumber;
                }
                string response = "You rolled a " + total + ". Your individual rolls were: " + string.Join(", ", numbers);
                if (numbers.Contains(1)) response += "\nNice nat 1, bozo";
                if (numbers.Contains(Int32.Parse(diceroll[1]))) response += "\nYou crit for once?";
                await ReplyAsync(response);
            }
        }
        [RequireOwner]
        [Command ("shutdown")]

        public async Task shutDown()
        {
            await ReplyAsync("Shutting down.");
        }
    }
}