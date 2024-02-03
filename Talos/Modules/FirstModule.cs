using Discord.Commands;

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
}