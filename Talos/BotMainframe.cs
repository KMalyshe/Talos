﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;

public class BotMainframe
{
    public static Task Main(string[] args) => new BotMainframe().MainAsync();
    private DiscordSocketClient? client;
	public async Task MainAsync()
	{
        client = new DiscordSocketClient();

        client.Log += Log;
        var token = File.ReadAllText(@"C:\TalosFiles\token.txt");

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        await Task.Delay(-1);
	}
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
