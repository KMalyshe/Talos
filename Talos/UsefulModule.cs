using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;

namespace TalosBot.Modules
{
    public class UsefulCommands : ModuleBase<SocketCommandContext>
    {
        [Command ("help")]
        public async Task help()
        {
            var embed = new EmbedBuilder();
            // !fish
            embed.AddField("*!fish*", "Cast your rod and retrieve a random fish. Maybe you'll get lucky!");
            // !fishcollection
            embed.AddField("*!fishcollection*", "Check on your lovely collection.");
            // !cat
            embed.AddField("*!cat*", "Post a random goofy cat gif.");
            // !roll
            embed.AddField("*!roll*", "Roll a DnD dice in the format of IntegerdInteger. Ex: 1d6.");
            // !8ball
            embed.AddField("*!8ball*", "Get an answer for life's most burning questions.");
            // !poe
            embed.AddField("*!poe alias !poesearch*", "Search the PoE wiki for the provided term.");
            // !ping
            embed.AddField("*!ping*", "Check if the bot lives.").WithColor(Color.Green);
            await ReplyAsync(embed: embed.Build());
        }
    }
}