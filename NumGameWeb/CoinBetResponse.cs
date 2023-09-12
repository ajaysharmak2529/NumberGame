namespace NumGameWeb {
    public class CoinBetResponse {
        public int id { get; set; }
        public int user_id { get; set; }
        public string? date { get; set; }
        public string? coin_type { get; set; }
        public string? type { get; set; }
        public int amount { get; set; }
        public string? user_token { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

}
