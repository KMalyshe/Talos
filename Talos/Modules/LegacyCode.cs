/* OLD FISHING -----------------------------------------------------------------------------------------------
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

OLD FISHING -----------------------------------------------------------------------------------------------*/