namespace FaceCheck.Server.Model
{
    public class HttpResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public int Code {  get; set; }
    }
    public class HttpResult<T> : HttpResult
    {
        public T Data { get; set; }
    }
}
