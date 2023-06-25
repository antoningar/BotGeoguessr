using Discord.WebSocket;

namespace BotGeoGuessr.Services
{
    internal interface IBotService
    {
        public Task MessageReceivedAsync(SocketMessage rawMessage);
    }
}
