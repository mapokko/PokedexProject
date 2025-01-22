using FluentValidation;
using System.Net;
using System.Text.Json.Serialization;

namespace PokedexProject.Models
{
    #region Model
    public class TranslationResponse
    {
        [JsonPropertyName("success")]
        public SuccessItemCounter SuccessItemCounter { get; set; }

        [JsonPropertyName("contents")]
        public Contents Contents { get; set; }

    }

    public class SuccessItemCounter
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class Contents
    {
        [JsonPropertyName("translated")]
        public string Translated { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("translation")]
        public string Translation { get; set; }
    }

    public class TranslationError
    {
        [JsonPropertyName("error")]
        public TranslationErrorContent Error { get; set; }
}

    public class TranslationErrorContent
    {
        [JsonPropertyName("code")]
        public HttpStatusCode Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    #endregion

    #region Validator

    public class TranslationResponseValidator : AbstractValidator<TranslationResponse>
    {
        private const string LANG_CODE = "en";
        public TranslationResponseValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SuccessItemCounter)
                .NotNull()
                .WithMessage("No Success Item found")
                .Must(x => x.Total > 0)
                .WithMessage("Success total is less than 1");


            RuleFor(x => x.Contents)
                .NotNull()
                .WithMessage("No Translation Content found")
                .Must(x => !string.IsNullOrEmpty(x.Translated))
                .WithMessage("Translation is null or empty");
        }
    }

    #endregion

}
