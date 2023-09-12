using System.Text.Json.Serialization;

namespace NumGameWeb.Data
{
    public class BettingInfo
    {
        public int user_id { get; set; }
        public int number { get; set; }
        public int amount { get; set; }
        public string? user_token { get; set; }
    }

}
