namespace NumGameWeb.Data
{
    public class ResponseResult<T> where T : class
    {
        public bool status { get; set; }
        public string message { get; set; }
        public T data { get; set; }
    }
}
