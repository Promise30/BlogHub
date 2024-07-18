namespace BloggingAPI.Contracts.Dtos.Responses
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public bool Status { get; set; }

        public static ApiResponse<T> Success(short statusCode, T? data, string? message)
        {
            return new ApiResponse<T>()
            {
                StatusCode = statusCode,
                Status = true,
                Data = data,
                Message = message
            };
        }
        public static ApiResponse<T> Success(short statusCode, string message)
        {
            return new ApiResponse<T>()
            {
                Status = true,
                StatusCode = statusCode,
                Message = message
            };
        }
        public static ApiResponse<T> Failure(short statusCode, T? data, string message)
        {
            return new ApiResponse<T>()
            {
                Status = false,
                StatusCode = statusCode,
                Data = data,
                Message = message

            };
        }
        public static ApiResponse<T> Failure(short statusCode, string message)
        {
            return new ApiResponse<T>()
            {
                Status = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
