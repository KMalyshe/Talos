using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
public class CommandHandler
{
    private readonly DiscordSocketClient client;
    private readonly CommandService commands;

    // Retrieve client and CommandService instance
    public CommandHandler(DiscordSocketClient client, CommandService commands)
    {
        this.commands = commands;
        this.client = client;
    }
    
    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into command handler
        client.MessageReceived += HandleCommandAsync;

        await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
    }
    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot)
        {
            return;
        }

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await commands.ExecuteAsync(context: context, argPos: argPos, services: null);
    }
}