namespace NetcoreApi.Models
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Data = data,
                Success = true,
                StatusCode = 200,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResponse(string error, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Error = error,
                Success = false,
                StatusCode = statusCode
            };
        }
    }
}
