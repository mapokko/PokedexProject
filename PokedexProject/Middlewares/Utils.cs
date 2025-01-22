using System.Net;
using System.Text.Json.Serialization;

namespace PokedexProject.Middlewares
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public List<string> ErrorMessages { get; set; } = [];
        public HttpStatusCode StatusCode { get; set; }

        public ErrorList Errors =>
            new()
            {
                Errors = ErrorMessages.Select(x => new Error { ErrorMessage = x }).ToList()
            };

        public static Result<T> SuccessResult(T data) => new Result<T> { Success = true, Data = data };
        public static Result<T> ErrorResult(HttpStatusCode code, string errorMessage) => new Result<T> { Success = false, ErrorMessages = new List<string>{errorMessage}, StatusCode = code};
        public static Result<T> ErrorResult(HttpStatusCode code, List<string> errorMessages) => new Result<T> { Success = false, ErrorMessages = errorMessages, StatusCode = code};
    }

    public class ErrorList
    {
        [JsonPropertyName("error_list")] 
        public List<Error> Errors { get; set; } = [];
    }

    public class Error
    {
        [JsonPropertyName("error_message")]
        public string ErrorMessage { get; set; }
    }

}

