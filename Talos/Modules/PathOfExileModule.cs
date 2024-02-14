using Discord.Commands;

namespace TalosBot.Modules
{
    public class PoECommands : ModuleBase<SocketCommandContext>
    {
        [Command ("poesearch")]
        [Alias ("poe")]

        public async Task PoeSearch(params string[] search)
        {
            // Capitalize the first letter of every word for the wiki
            var reply = "";
            foreach (string word in search)
            {
                if (word.Length == 1)
                {
                    reply += word.ToUpper() + "_";
                    continue;
                }
                reply += char.ToUpper(word.First()) + word.Substring(1).ToLower() + "_";
            }
            if (reply.Last() == '_') reply = reply.Remove(reply.Length - 1, 1);
            await ReplyAsync("https://www.poewiki.net/wiki/" + reply);
            
        }
    }
}