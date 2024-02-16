using Discord.Commands;

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
            await ReplyAsync(reply);
        }
    }
}