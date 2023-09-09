namespace NumGameWeb.Data
{
    public class UserDetail
    {
        public bool status { get; set; }
        public string? message { get; set; }
        public UserData? data { get; set; }
    }


    public class UserData
    {
        public int id { get; set; }
        public string? full_name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public decimal wallet { get; set; }
    }

}

