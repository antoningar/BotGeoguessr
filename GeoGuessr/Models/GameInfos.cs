namespace BotGeoGuessr.GeoGuessr.Models
{
    public class GameInfos
    {
        public int PlayerNumber { get; set; }
        public string HostId { get; set; }
        public string PartyId { get; set; }

        public GameInfos(int playerNumber, string hostId, string partyId)
        {
            PlayerNumber = playerNumber;
            HostId = hostId;
            PartyId = partyId;
        }
    }
}
