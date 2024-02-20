using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;

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
                command.Parameters.AddWithValue("$name", user.Username);
                command.ExecuteNonQuery();

                command.CommandText =
                @$"
                UPDATE UserCollection
                SET {fishcaught} = {fishcaught} + 1
                WHERE USERID = @name
                ";
                command.Parameters.AddWithValue("@name", user.Username);
                command.ExecuteNonQuery();

            }
            await File.AppendAllTextAsync(@"C:\TalosFiles\fishlog.txt", user.Username + " caught a " + fishcaught + " at " + DateTime.Now + "." + Environment.NewLine);
        }

        [Command ("fishcollection")]

        public async Task fishCollection(SocketUser user = null)
        {
            using (var connection = new SqliteConnection(@"Data Source=C:\TalosFiles\SQL\fish.db"))
            {
                List<int> fishAmounts = new List<int>();

                connection.Open();
                var command = connection.CreateCommand();
                var userinfo = user ?? Context.User;
                command.CommandText = @"SELECT T1FISH, T2FISH, T3FISH, T4FISH, T5FISH FROM UserCollection WHERE USERID = $name;";
                command.Parameters.AddWithValue("$name", userinfo.Username);
                SqliteDataReader reader = command.ExecuteReader();
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
        }
    }
}