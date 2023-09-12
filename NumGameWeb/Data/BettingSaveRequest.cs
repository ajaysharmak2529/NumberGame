namespace NumGameWeb.Data {
    public class BettingSaveRequest {
        public string? token { get; set; }
        public string? created_at { get; set; }
        public List<Betting>? bettingInfo { get; set; } = new List<Betting>();
    }

}
