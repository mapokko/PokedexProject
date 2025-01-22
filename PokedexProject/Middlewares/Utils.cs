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

    public class PokemonDisplay : PokemonInfo
    {
        public PokemonDisplay() { }
        public PokemonDisplay(PokemonCache pokemon, TranslationType type)
        {
            Name = pokemon.Name;
            IsLegendary = pokemon.IsLegendary;
            HabitatName = pokemon.HabitatName;

            switch (type)
            {
                case TranslationType.Regular:
                    Description = pokemon.RegularText;
                    break;
                case TranslationType.Yoda:
                    Description = pokemon.YodaTranslation;
                    break;
                case TranslationType.Shakespeare:
                    Description = pokemon.ShakespeareTranslation;
                    break;
            }
        }

    }

    public class PokemonCache : PokemonInfo
    {
        public PokemonCache(PokemonDisplay pokemon)
        {
            Name = pokemon.Name;
            IsLegendary = pokemon.IsLegendary;
            HabitatName = pokemon.HabitatName;
            RegularText = pokemon.Description;
        }
        public string RegularText { get; set; }
        public string YodaTranslation { get; set; }
        public string ShakespeareTranslation { get; set; }
    }

    public class PokemonInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("is_legendary")]
        public bool IsLegendary { get; set; }

        [JsonPropertyName("habitat")]
        public string HabitatName { get; set; }

        [JsonIgnore]
        public bool UseYoda => IsLegendary || HabitatName == "cave";
    }

    public enum TranslationType
    {
        Regular,
        Yoda,
        Shakespeare
    }

}

