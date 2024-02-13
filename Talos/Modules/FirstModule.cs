using Discord;
using Discord.Commands;

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
            await Context.Channel.SendMessageAsync("You entered: " + num + ". Your number doubled equals " + num*2 + ".");
        }
    }
}