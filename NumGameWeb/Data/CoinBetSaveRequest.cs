namespace NumGameWeb.Data {
    public class CoinBetSaveRequest {
        public string? token { get; set; }
        public string? created_at { get; set; }
        public List<CoinBet>? bettingInfo { get; set; } = new List<CoinBet>();
    }

}
