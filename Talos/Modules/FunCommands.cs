using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using KaimiraGames;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Discord.Rest;
namespace TalosBot.Modules
{
    public class FunCommands : ModuleBase<SocketCommandContext>
    {
        [Command ("8ball")]
        public async Task eightBall(params string[] search) 
        {
            var url = "https://www.eightballapi.com/api?question=&lucky=false";
            string reply = "";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var fetch = await response.Content.ReadAsStringAsync();
                    fetch = fetch.Split("\":\"")[1];
                    reply = fetch.Remove(fetch.Length-2);
                }
            }

            var embed = new EmbedBuilder();
            embed.AddField("The ball's infinite wisdom proclaims the answer.", reply)
            .WithCurrentTimestamp();
            await ReplyAsync(embed: embed.Build());
        }


        [Command ("fish")]

        public async Task fish(SocketUser user = null)
        {
            // Set the User and make sure it's not for someone else
            if (user != null)
            {
                await ReplyAsync("You can't fish for someone else!");
                return;
            }
            else user = Context.User;

            //Check cooldown data table
            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                //Fetch all cooldown entries for user
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT time FROM cooldown WHERE userid = $id";
                command.Parameters.AddWithValue("$id", user.Id);
                var timeReader = command.ExecuteReader();
                command.Parameters.Clear();

                //If no rows, just add a row with current UTC time
                if (!timeReader.HasRows)
                {
                    timeReader.Close();
                    command.CommandText = @"INSERT INTO cooldown (userid, time) VALUES (@id, @currtime)";
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@currtime", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }


                else
                {
                    //List to track amount of entries and also keep the oldest entry
                    List<string> times = new List<string>();
                    while(timeReader.Read())
                    {
                        times.Add(timeReader[0].ToString());
                    }

                    // If there are 8 cooldown entries
                    if (times.Count() > 7)
                    {
                        long thenTime = Convert.ToInt64(times[0]);
                        long currDateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


                        if(currDateTime - thenTime < 3600) // If less than 3600 seconds have passed from oldest entry
                        {
                            var nextFish = thenTime + 3600;
                            await ReplyAsync($"<@{user.Id}>, you are on cooldown! You may only fish eight times per hour. You may fish again <t:{nextFish}:R>.");
                            return;
                        }


                        else // If 3600 or more seconds have passed since oldest fishing attempt
                        // Delete oldest entry and insert new one with current time
                        {
                            timeReader.Close();
                            command.Parameters.Clear();
                            command.CommandText = @"DELETE FROM cooldown WHERE id = (SELECT id FROM cooldown WHERE userid = $id2 LIMIT 1)";
                            command.Parameters.AddWithValue("$id2", user.Id);
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = @"INSERT INTO cooldown (userid, time) VALUES ($id, $currtime)";
                            command.Parameters.AddWithValue("$id", user.Id);
                            command.Parameters.AddWithValue("$currtime", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                            command.ExecuteNonQuery();
                        }
                    }

                    // Less than 8 cooldown entries, just add one
                    else 
                    {
                        timeReader.Close();
                        command.CommandText = @"INSERT INTO cooldown (userid, time) VALUES ($id, $currtime)";
                        command.Parameters.AddWithValue("$id", user.Id);
                        command.Parameters.AddWithValue("$currtime", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        command.ExecuteNonQuery();
                    }
                }
                // End cooldown block

                // Start fishing block
                command.Parameters.Clear();

                // Which fish is being caught (loop to also determine category easily)
                Random rnd = new Random();
                WeightedList<string> fishWeights = new();
                var fishList = new List<int>(){92, 80, 60, 35, 0};
                var whichcategory = 0;
                var randomFish = rnd.Next(100);
                for (int i = 0; i<fishList.Count(); i++)
                {
                    if (randomFish >= fishList[i]) 
                    {
                    whichcategory = i+1;
                    break;
                    }
                }

                // Legacy code, WeightedList no longer necessary but whatever
                // fetch the fish name and the pathname of the file
                command.CommandText = @$"SELECT FISHNAME FROM fishinfo WHERE FISHWEIGHT = {whichcategory};";
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    fishWeights.Add(reader[0].ToString(), 1);
                }
                reader.Close();
                var fishname = fishWeights.Next();
                var path = fishname.Replace('_', '-');
                
                // Has user found this fish before?
                command.Parameters.Clear();
                command.CommandText = @$"SELECT * FROM userinfo WHERE FISHID = @name AND USERID = @username";
                command.Parameters.AddWithValue("@name", fishname);
                command.Parameters.AddWithValue("@username", user.Id);
                var reader2 = command.ExecuteReader();
                command.Parameters.Clear();

                if (!reader2.HasRows) // If not, insert row with count 1
                {
                    reader2.Close();
                    command.CommandText = @"INSERT INTO userinfo (USERID, FISHID, COUNT) VALUES ($username, $name, 1);";
                    command.Parameters.AddWithValue("$username", user.Id);
                    command.Parameters.AddWithValue("$name", fishname);
                    command.ExecuteNonQuery();
                }

                else // If yes, update count by 1
                {
                    reader2.Close();
                    command.CommandText = @"UPDATE userinfo SET COUNT = COUNT+1 WHERE USERID = $username AND FISHID = $name;";
                    command.Parameters.AddWithValue("$username", user.Id);
                    command.Parameters.AddWithValue("$name", fishname);
                    command.ExecuteNonQuery();
                }

                // Formatting
                string caughtfish = path.Remove(path.Length-1).Replace('-', '_');

                if (caughtfish.Contains("_")) {
                    string tmp = "";
                    foreach (string word in caughtfish.Split("_"))
                    {
                        tmp += char.ToUpper(word.First()) + word.Substring(1).ToLower() + " ";
                    }
                    tmp = tmp.Remove(tmp.Length-1);
                    caughtfish = tmp;
                }
                else caughtfish = char.ToUpper(caughtfish.First()) + caughtfish.Substring(1).ToLower();
                var article = "a";
                if ("AEIOU".Contains(caughtfish[0])) article+="n";

                // What to write depending on fish category
                var categoryReactions = new Dictionary<int, string>(){
                    {1, "What an incredible, brilliant catch! Bards will sing songs about this capture. \n⭐⭐⭐⭐⭐"},
                    {2, "A catch to be proud of. Congratulations! You're a veteran fisher. \n⭐⭐⭐⭐"},
                    {3, "Impressive! But there's always a bigger fish. \n⭐⭐⭐"},
                    {4, "A nice surprise! You're movin' up in the world! \n⭐⭐"},
                    {5, "Well, at least it's still a fish. \n⭐"}
                };

                // How much score
                var categoryScore = new Dictionary<int, int>(){
                    {1, 100},
                    {2, 45},
                    {3, 20},
                    {4, 15},
                    {5, 5}
                };

                // If user has no score, add user row
                command.Parameters.Clear();
                command.CommandText = @"INSERT OR IGNORE INTO fishleaderboard (userid, score) VALUES (@id, 0)";
                command.Parameters.AddWithValue("@id", user.Id);
                command.ExecuteNonQuery();

                // Add score to user score in DB
                command.Parameters.Clear();
                command.CommandText = @"UPDATE fishleaderboard SET score = score + @num WHERE userid = @id";
                command.Parameters.AddWithValue("@num", categoryScore[whichcategory]);
                command.Parameters.AddWithValue("@id", user.Id);
                command.ExecuteNonQuery();

                // Heart of the Depths
                var heart = rnd.Next(100);

                var filename = Path.GetFileName(@$"C:\TalosFiles\fishes\fishes\icons\{path}.png");

                if (heart < 5) // If heart of the depths has been found, not sure if i can make the if statement smaller, AddField is weird
                {
                    var embedder = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTimestamp(DateTime.Now) 
                    .AddField("You cast your mighty rod into the endless void...", $"... and catch {article} **{caughtfish}**! " + categoryReactions[whichcategory])
                    .WithImageUrl($"attachment://{filename}")
                    .WithFooter("Invoked by " + user.Username)
                    .AddField("🌀 Wow! You found a Heart of the Depths! 🌀", "Your fishing cooldowns have been reset.")
                    .Build();

                    // Wipe cooldoown for user
                    command.Parameters.Clear();
                    command.CommandText = @"DELETE FROM cooldown WHERE id IN (SELECT id FROM cooldown WHERE userid = $id2 LIMIT 8)";
                    command.Parameters.AddWithValue("$id2", user.Id);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();

                    await Context.Channel.SendFileAsync(@$"C:\TalosFiles\fishes\fishes\icons\{path}.png", null, false, embedder);
                }
                else
                {
                    var embedder = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTimestamp(DateTime.Now) 
                    .AddField("You cast your mighty rod into the endless void...", $"... and catch {article} **{caughtfish}**! " + categoryReactions[whichcategory])
                    .WithImageUrl($"attachment://{filename}")
                    .WithFooter("Invoked by " + user.Username)
                    .Build();
                    await Context.Channel.SendFileAsync(@$"C:\TalosFiles\fishes\fishes\icons\{path}.png", null, false, embedder);
                }
            }
            

        }
        

        [Command ("fishcollection")]

        public async Task fishCollection(string tier = "all", SocketUser? user = null)
        {
            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                var fishAmounts = new Dictionary<string, int>();
                connection.Open();
                var command = connection.CreateCommand();
                var userinfo = user ?? Context.User;

                // Check valid input
                if (tier != "all")
                {
                    if (!"12345".Contains(tier) || (tier.Length > 1))
                    {
                        await ReplyAsync("Please enter a valid fish star level, or enter all to view your entire collection.");
                        return;
                    }
                }


                command.CommandText = @$"SELECT FISHID, COUNT FROM userinfo WHERE USERID = @name";
                command.Parameters.AddWithValue("@name", userinfo.Id);
                SqliteDataReader reader = command.ExecuteReader();
                command.Parameters.Clear();


                if (!reader.HasRows)
                {
                    await ReplyAsync("User has not fished before.");
                    return;
                }


                while (reader.Read()) {
                    string fishname = reader[0].ToString();

                    // If fish not in requested tier
                    if ((tier != "all") && !fishname.Contains((6-Int32.Parse(tier)).ToString())) continue;

                    // Fish name formatting
                    if (fishname.Contains("_")) {
                    string fishFormat = "";
                    foreach (string word in fishname.Split("_"))
                    {
                        fishFormat += char.ToUpper(word.First()) + word.Substring(1).ToLower() + " ";
                    }
                    fishFormat = fishFormat.Remove(fishFormat.Length-1);
                    fishname = fishFormat;
                    }
                    else fishname = char.ToUpper(fishname.First()) + fishname.Substring(1).ToLower();


                    fishAmounts.Add(fishname, Convert.ToInt32(reader[1]));
                }


                if (tier == "all")
                {
                    List<List<string>> fishList = new List<List<string>>();

                    // Add all fish into a list for each tier
                    for (int i = 0; i<5; i++) fishList.Add(new List<string>());
                    foreach(string fish in fishAmounts.Keys)
                    {
                        var intTier = ((int) Char.GetNumericValue(fish[fish.Length-1]))-1;
                        fishList[intTier].Add(fish.Remove(fish.Length-1));
                    }

                    var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithFooter("Invoked by " + Context.User.Username)
                    .WithTimestamp(DateTime.Now)
                    .WithTitle($"{userinfo.Username}'s Fish Collection");

                    var currTier = 1;
                    var totalfish = 0;


                    foreach(List<string> tiers in fishList) // For each star tier
                    {
                        string fishes = "";
                        foreach(string fish in tiers) // For each fish type
                        {
                            var fishcount = fishAmounts[fish + currTier.ToString()];
                            totalfish += fishcount;

                            // Formatting
                            fishes+=fish + ": " + "**" + fishcount + Environment.NewLine + "**";
                        }
                        if (fishes.Equals("")) fishes = "You have not found any \nfish of this tier!";
                        embed.AddField($"{String.Concat(Enumerable.Repeat("⭐", 6-currTier))} \nFish:", fishes, true);
                        currTier++;
                    }
                    embed.AddField("Total fish collected:", totalfish.ToString());
                    await ReplyAsync(embed: embed.Build());
                }
                


                else // for specific star tier
                {
                    List<string> fishList = new List<string>();
                    reader.Close();
                    command.Parameters.Clear();
                    command.CommandText = @"SELECT FISHNAME FROM fishinfo WHERE FISHWEIGHT = $weight";
                    int whichTier = 6-Int32.Parse(tier);
                    command.Parameters.AddWithValue("$weight", whichTier);
                    var reader2 = command.ExecuteReader();
                    command.Parameters.Clear();


                    while (reader2.Read())
                    {
                        string fishname = reader2[0].ToString();

                        // Formatting
                        if (fishname.Contains("_")) 
                        {
                            string tmp = "";
                            foreach (string word in fishname.Split("_"))
                            {
                                tmp += char.ToUpper(word.First()) + word.Substring(1).ToLower() + " ";
                            }
                            tmp = tmp.Remove(tmp.Length-1);
                            fishname = tmp;
                        }
                        else fishname = char.ToUpper(fishname.First()) + fishname.Substring(1).ToLower();


                        fishList.Add(fishname);
                    }


                    var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithFooter("Invoked by " + Context.User.Username)
                    .WithTimestamp(DateTime.Now)
                    .WithTitle($"{userinfo.Username}'s {String.Concat(Enumerable.Repeat("⭐", Int32.Parse(tier)))} Fish Collection");
                    var fishString = "";


                    foreach(string fish in fishList)
                    {
                        var fishNoNumber = fish.Remove(fish.Length-1);
                        if (!fishAmounts.Keys.Contains(fish)) fishString += $"*{fishNoNumber}: Not found yet!*" + Environment.NewLine;
                        else fishString += $"**{fishNoNumber}**: {fishAmounts[fish]}" + Environment.NewLine; 
                    }


                    embed.AddField("To find out more about a certain \nfish, do !fishinfo. **WIP!; NOT DONE YET, COMING SOON**", fishString);
                    await ReplyAsync(embed: embed.Build());
                }
            }
        }


        [Command ("fishscore")]

        public async Task fishScore(SocketUser? user = null)
        {

            var userinfo = user ?? Context.User;
            var score = 0;


            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT score FROM fishleaderboard WHERE userid = @id";
                command.Parameters.AddWithValue("@id", userinfo.Id);
                var reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    score = 0; // TODO: Update to say "User hasn't fished before" instead of 0 score
                }
                else 
                {
                    while (reader.Read())
                    {
                        score = Convert.ToInt32(reader[0]);
                    }
                }
            }


            var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithFooter("Invoked by " + Context.User.Username)
            .WithTimestamp(DateTime.Now)
            .AddField($"{userinfo.Username}'s Fishing Score:", $"**{score}**");
            await ReplyAsync(embed: embed.Build());
        }


        [Command ("fishlb")]
        [Alias ("fishleaderboard")]

        public async Task fishLeaderboard(string num = null)
        {

            // Check to make sure input is valid
            if (num == null) num = "5";
            else if (string.IsNullOrWhiteSpace(num.Trim('0')))
            {
                await ReplyAsync("You can't have me display zero entries.");
                return;
            }
            else if (!Regex.IsMatch(num, @"^\d+$"))
            {
                await ReplyAsync("Input is not a number.");
                return;
            }
            else if (!((Int32.Parse(num) <= 10) && (Int32.Parse(num) > 0)))
            {
                await ReplyAsync("Invalid leaderboard. Valid inputs range from 1-10.");
                return;
            }


            var str = "";
            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT userid, score FROM fishleaderboard ORDER BY score DESC LIMIT @num";
                command.Parameters.AddWithValue("@num", Int32.Parse(num));
                var reader = command.ExecuteReader();


                List<Tuple<string, int>> scores = new List<Tuple<string, int>>();
                while (reader.Read())
                {
                    scores.Add((reader[0].ToString(), Convert.ToInt32(reader[1])).ToTuple());
                }
                var tracker = 1;
                foreach (Tuple<string, int> score in scores)
                {
                    str += $"**{tracker}** <@{score.Item1}>: {score.Item2}" + Environment.NewLine;
                    tracker++;
                }
            }


            var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithFooter("Invoked by " + Context.User.Username)
            .WithTimestamp(DateTime.Now)
            .AddField($"Fishing Leaderboard", str);

            await ReplyAsync(embed: embed.Build());
        }
    }
}