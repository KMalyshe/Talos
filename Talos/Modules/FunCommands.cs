using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using KaimiraGames;
using System.Collections.Specialized;
namespace TalosBot.Modules
{
    public class FunCommands : ModuleBase<SocketCommandContext>
    {
        [Command ("8ball")]
        public async Task eightBall(params string[] search) 
        {
            /*var query = "";
            for(int i = 0; i<search.Length; i++)
            {
                if (i == 0) query += char.ToUpper(search[0].First()) + search[0].Substring(1).ToLower() + " ";
                else if (i == search.Length-1) query += search[search.Length-1] + "?";
                else query += search[i] + " ";
            }
            var answer = query;*/
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
            if (user != null)
            {
                await ReplyAsync("You can't fish for someone else!");
                return;
            }
            /*
            Random rng = new Random();
            var fishDict = new Dictionary<int, string>(){
                {12, "T5FISH"},
                {6, "T4FISH"},
                {3, "T3FISH"},
                {1, "T2FISH"},
                {0, "T1FISH"}
            };
            var fish = rng.Next(20);
            var fishcaught = "";
            foreach (int key in fishDict.Keys)
            {
                if (fish >= key)
                {
                    fishcaught = fishDict[key];
                    break;
                }
            }
            var embed = new EmbedBuilder();
            var fishParsing = new Dictionary<string, string>(){
                {"T1FISH", "... 🐠. What an incredible catch! You should hit the lotto today."},
                {"T2FISH", "... 🦐. It's that shrimple."},
                {"T3FISH", "... 🐟. At least it's a fish."},
                {"T4FISH", "... 🔋. It's still charged, somehow."},
                {"T5FISH", "... 👢. Better luck next time, cowboy."}
            };
            var whichFish = fishParsing[fishcaught];
            embed.AddField("You cast your mighty rod into the Discordian depths and catch...", whichFish).WithColor(Color.Blue);
            await ReplyAsync(embed: embed.Build());
            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                if (user == null) user = Context.User;
                command.CommandText = 
                @"
                INSERT OR IGNORE INTO UserCollection(USERID, T1FISH, T2FISH, T3FISH, T4FISH, T5FISH) VALUES($name, 0, 0, 0, 0, 0)
                ";
                command.Parameters.AddWithValue("$name", user.Id.ToString());
                command.ExecuteNonQuery();

                command.CommandText =
                @$"
                UPDATE UserCollection
                SET {fishcaught} = {fishcaught} + 1
                WHERE USERID = @name
                ";
                command.Parameters.AddWithValue("@name", user.Id.ToString());
                command.ExecuteNonQuery();

            }
            await File.AppendAllTextAsync(@"C:\TalosFiles\fishlog.txt", user.Username + " caught a " + fishcaught + " at " + DateTime.Now + "." + Environment.NewLine);
            */
            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                if (user == null) user = Context.User;
                Random rnd = new Random();
                WeightedList<string> fishWeights = new();
                connection.Open();
                var command = connection.CreateCommand();
                var fishList = new List<int>(){92, 90, 60, 35, 0};
                var whichcategory = 0;
                var fish2 = rnd.Next(100);
                for (int i = 0; i<fishList.Count(); i++)
                {
                    if (fish2 >= fishList[i]) 
                    {
                    whichcategory = i+1;
                    break;
                    }
                }
                command.CommandText = @$"SELECT FISHNAME FROM fishinfo WHERE FISHWEIGHT = {whichcategory};";
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    fishWeights.Add(reader[0].ToString(), 1);
                }
                reader.Close();
                var fishname = fishWeights.Next();
                var path = fishname.Replace('_', '-');
                /* PROBLEMATIC SQL:
                command.CommandText = @$"
                CASE WHEN
                    NOT EXISTS 
                        (SELECT 1 FROM userinfo 
                        WHERE USERID = {user.Id} AND 
                        FISHID = (SELECT FISHID FROM fishinfo WHERE FISHNAME = {fishname}))
                        THEN
                            INSERT INTO userinfo (USERID, FISHID, COUNT) VALUES ({user.Id}, (SELECT FISHID FROM fishinfo WHERE FISHNAME = {fishname})), 1)
                ELSE
                    UPDATE userinfo SET COUNT = COUNT+1 WHERE USERID = {user.Id} AND FISHID = (SELECT FISHID FROM fishinfo WHERE FISHNAME = {fishname})
                END
                ;";
                command.ExecuteNonQuery();
                */
                command.CommandText = @$"SELECT * FROM userinfo WHERE FISHID = @name AND USERID = @username";
                command.Parameters.AddWithValue("@name", fishname);
                command.Parameters.AddWithValue("@username", user.Id);
                var reader2 = command.ExecuteReader();
                if (!reader2.HasRows)
                {
                    reader2.Close();
                    command.CommandText = @"INSERT INTO userinfo (USERID, FISHID, COUNT) VALUES ($username, $name, 1);";
                    command.Parameters.AddWithValue("$username", user.Id);
                    command.Parameters.AddWithValue("$name", fishname);
                    command.ExecuteNonQuery();
                }
                else
                {
                    reader2.Close();
                    command.CommandText = @"UPDATE userinfo SET COUNT = COUNT+1 WHERE USERID = $username AND FISHID = $name;";
                    command.Parameters.AddWithValue("$username", user.Id);
                    command.Parameters.AddWithValue("$name", fishname);
                    command.ExecuteNonQuery();
                }

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
                var categoryReactions = new Dictionary<int, string>(){
                    {1, "What an incredible, brilliant catch! Bards will sing songs about this capture."},
                    {2, "A catch to be proud of. Congratulations! You're a veteran fisher."},
                    {3, "Impressive! But there's always a bigger fish.."},
                    {4, "A nice surprise! You're movin' up in the world!"},
                    {5, "Well, at least it's still a fish."}
                };


                var filename = Path.GetFileName(@$"C:\TalosFiles\fishes\fishes\icons\{path}.png");
                var embedder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTimestamp(DateTime.Now) 
                .AddField("You cast your mighty rod into the endless void...", $"... and catch {article} **{caughtfish}**! " + categoryReactions[whichcategory])
                .WithImageUrl($"attachment://{filename}")
                .WithFooter("Invoked by " + user.Username)
                .Build();
                await Context.Channel.SendFileAsync(@$"C:\TalosFiles\fishes\fishes\icons\{path}.png", null, false, embedder);
                // await ReplyAsync(whichcategory.ToString());
            }
            

        }
        /*
        [Command ("fishcollection")]

        public async Task fishCollection(SocketUser? user = null)
        {
            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                List<int> fishAmounts = new List<int>();
                connection.Open();
                var command = connection.CreateCommand();
                var userinfo = user ?? Context.User;
                command.CommandText = @"SELECT T1FISH, T2FISH, T3FISH, T4FISH, T5FISH FROM UserCollection WHERE USERID = $name;";
                command.Parameters.AddWithValue("$name", userinfo.Id.ToString());
                SqliteDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    await ReplyAsync("User has not fished before.");
                    return;
                }
                if (reader.Read()) {
                    fishAmounts.Add(Convert.ToInt32(reader[0]));
                    fishAmounts.Add(Convert.ToInt32(reader[1]));
                    fishAmounts.Add(Convert.ToInt32(reader[2]));
                    fishAmounts.Add(Convert.ToInt32(reader[3]));
                    fishAmounts.Add(Convert.ToInt32(reader[4]));
                }
                var embed = new EmbedBuilder();
                embed.AddField(userinfo.Username + "'s", "Marvelous Fishing Collection");
                var fishDict = new Dictionary<int, string>()
                {
                {0, "🐠 \nThe most tropical of fish. \n**"},
                {1, "🦐 \nIt could not be more shrimple. \n**"},
                {2, "🐟 \nThe kind you'd find at the market. \n**"},
                {3, "🔋 \nYou can charge something, I guess? \n**"},
                {4, "👢 \nEventually, you'll find the right size. \n**"}
                };
                for (int i = 0; i<5; i++)
                {
                    embed.AddField("Tier " + (i+1) + " Fish:", fishDict[i] + fishAmounts[i].ToString() + "**");
                }
                embed.AddField("Total Fish Accrued:", "**" + fishAmounts.Sum() + "**").WithColor(Color.Blue);
                await ReplyAsync(embed: embed.Build());
            }
        }*/
    }
}