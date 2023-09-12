namespace NumGameWeb.Data
{
    public class UserHistoryResponse {
        public bool status { get; set; }
        public string? message { get; set; }
        public int row_count { get; set; }
        public List<UserHistoryObj>? data { get; set; }
    }  

    public class UserHistoryObj {
        public int id { get; set; }
        public int user_id { get; set; }
        public string? amount { get; set; }
        public string? number { get; set; }
        public string? type { get; set; }
        public int status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

}
