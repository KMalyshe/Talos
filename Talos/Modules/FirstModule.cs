using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Security.Cryptography;
using System.Windows.Input;
using System.Diagnostics;

namespace TalosBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("pong");
        }

        [Command("dog")]
        // i hate this, lazy implementation for a friend;
        public async Task dogPosting()
        {
            string[] gifOptions = [
                "https://tenor.com/view/what-da-dog-doin-what-da-dog-doin-gif-22078277",
                "https://tenor.com/view/bobawooyo-dog-confused-dog-huh-dog-meme-shocked-dog-gif-16713203299056947073",
                "https://tenor.com/view/harvest-yeahilif-gif-21827635",
                "https://tenor.com/view/the-harvest-is-lacking-this-year-harvest-lacking-dog-what-the-dog-doin-gif-21904000",
                "https://tenor.com/view/bubble-dog-chihuahua-rat-dog-bubble-dog-gif-20658904",
                "https://tenor.com/view/dog-smirk-dog-smirking-evil-gif-1174039551531775064",
                "https://tenor.com/view/doggy-golde-retriever-gif-6626272048167568250",
                "https://tenor.com/view/dog-dog-with-butterfly-on-nose-dog-with-butterfly-butterfly-gif-13282441872748349107",
                "https://tenor.com/view/phone-dog-dog-phone-hello-dog-calling-gif-4537530900414073988",
                "https://tenor.com/view/look-at-aurum-dog-aurum-so-cute-gif-4637197256753416402",
                "https://tenor.com/view/dog-hell-nah-hell-naw-button-gif-658894236480786400",
                "https://tenor.com/view/byuntear-meme-react-funny-dog-gif-25019867",
                "https://tenor.com/view/dog-gif-22639799",
                "https://tenor.com/view/kleyton-loko-gif-216549716130789892",
                "https://tenor.com/view/splooshysploosher-funny-dog-doge-doge-stare-dog-look-gif-26301702",
                "https://tenor.com/view/dog-slap-punch-fast-gif-5113397"
            ];
            Random rng = new Random();
            await ReplyAsync(gifOptions[rng.Next(gifOptions.Length)]);
        }
        public class UrlResult
        {
            public string url { get; set; }
        }
        [Command("cat")]
        public async Task catPosting(SocketUser user = null)
        {
            Random rng = new Random();
            // List of search options, handpicked, cat+Letter is in twice to make sure a specific letter isnt highly unlikely
            string[] searchOptions = ["cat-" + (char) (65 + rng.Next(25)), "this-cat-is-so", "cat-eepy", "cat-caption", "cat", "cat-goofy", "cat-voices", 
            "cat-crazy", "cat-stare", "but-heres-the", "cat-angry", "post-this-cat", "kitten",
            "orange-cat", "cat-meme", "cat-punch", "cat-smack", "cat-slap", "cat-crime",
            "angry-kitten", "cat-kitten", "cat-dance", "cat-skateboard", "cat-drive", "cat-meow", "cat-" + (char) (65 + rng.Next(25))];

            var searchOptionChosen = rng.Next(searchOptions.Length);
            string actualSearch = searchOptions[searchOptionChosen];
            var url = "https://tenor.googleapis.com/v2/search?q=" + actualSearch + "&key=" + 
            File.ReadAllText(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, @"TalosFiles\tenortoken.txt")) + "&client_key=talosbot&limit=10&media_filter=gif,tinygif";

            // Fetch gif from Tenor based on search url
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // Parse JSON response
                    var fetch = await response.Content.ReadAsStringAsync();
                    JObject search = JObject.Parse(fetch);
                    IList<JToken> results = search["results"].Children().ToList();
                    IList<UrlResult> urlResults = new List<UrlResult>();
                    foreach (JToken result in results)
                    {
                        UrlResult urlResult = result.ToObject<UrlResult>();
                        urlResults.Add(urlResult);
                    }

                    if (user == null) user = Context.User;
                    var resultOptionChosen = rng.Next(urlResults.Count);

                    // Add catpost to log
                    await File.AppendAllTextAsync(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, @"TalosFiles\catlog.txt"), DateTime.Now + " " + user.Username + 
                    ": Search Option: " + searchOptions[searchOptionChosen] + " Result Option: " + resultOptionChosen + Environment.NewLine);

                    await ReplyAsync(urlResults[resultOptionChosen].url);
                    
                    
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

            // Valid input?
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

                await ReplyAsync(response);
            }
        }
        [RequireOwner]
        [Command ("shutdown")]

        public async Task shutDown()
        {
            var author = Context.Message.Author;
            if (author.Id != ulong.Parse(File.ReadAllText(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName, @"TalosFiles\myid.txt")))) await ReplyAsync("no");
            else 
            {
                await ReplyAsync("Shutting down.");
                Process.GetCurrentProcess().Kill();
            }
            
        }
    }
}