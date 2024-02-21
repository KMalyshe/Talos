using Discord;
using Discord.Commands;
using PokeApiNet;
using Microsoft.Data.Sqlite;
using Type = PokeApiNet.Type;

namespace TalosBot.Modules
{
    public class PokemonCommands : ModuleBase<SocketCommandContext>
    {
        [Command ("dex")]
        [Alias ("pokemon", "pokedex")]
        public async Task pokeDex(params string[] search)
        {
            PokeApiClient pokeClient = new PokeApiClient();
            var actualsearch = "";
            if (search.Length > 1)
            {
                foreach (string word in search) actualsearch+=word;
            }
            else actualsearch = search[0];
            try 
            {
                var embed = new EmbedBuilder();
                Pokemon result = await pokeClient.GetResourceAsync<Pokemon>(actualsearch);

                var pokemonName = result.Name;

                var pokemonType = result.Types[0].Type.Name;
                if (result.Types.Count() > 1) pokemonType += ", " + result.Types[1].Type.Name;

                var pokemonSpecies = result.Species.Name;

                var pokemonNumber = result.GameIndicies.Last().GameIndex.ToString();
                while (pokemonNumber.Length < 3) pokemonNumber = "0"+pokemonNumber;
                pokemonNumber = "#" + pokemonNumber;
                
                
                embed.AddField(pokemonName + " | " + pokemonSpecies + " | " + pokemonNumber, pokemonType);
                await ReplyAsync(embed: embed.Build());
            }
            catch
            {
                await ReplyAsync("An error has occured. Did you spell the Pokemon correctly?");
            }

        }
    }
}