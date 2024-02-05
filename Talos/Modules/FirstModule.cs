using Discord.Commands;
using Discord.WebSocket;

public class InfoModule : ModuleBase<SocketCommandContext>
{
    [Command("say")]
	[Summary("Echoes a message.")]
    // say command to say something
	public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
		=> ReplyAsync(echo);
    // square a number
	[Command("square")]
	[Summary("Squares a number.")]
	public async Task SquareAsync(
		[Summary("The number to square.")] 
		int num)
	{
		await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
	}

	[Command("userinfo")]
	[Summary
	("Returns user info")]
	[Alias("user", "whois")]
	public async Task UserInfoAsync(
		[Summary("The (optional) user to get info from")]
		SocketUser? user = null)
	{
		var userInfo = user ?? Context.Client.CurrentUser;
		await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
	}
}