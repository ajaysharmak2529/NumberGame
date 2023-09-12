namespace NumGameWeb.Data {
    public class UserHistoryRequest {
        public string? token { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
    }   

}
