using System.Text.Json.Serialization;
using FluentValidation;
using PokedexProject.Middlewares;

namespace PokedexProject.Models
{
    #region Model

    public class PokemonDescription
    {
        [JsonPropertyName("is_legendary")]
        public bool IsLegendary { get; set; }

        [JsonPropertyName("flavor_text_entries")]
        public List<FlavorTextEntry> FlavorTextEntries { get; set; }

        [JsonPropertyName("names")]
        public List<NameEntry> NameEntries { get; set; }

        [JsonPropertyName("habitat")]
        public Entry Habitat { get; set; }

        // should be guaranteed not nullable by the validator
        [JsonIgnore]
        public string EnglishName {
            get
            {
                return NameEntries?.Where(el => el?.Language?.Name is "en")?.Select(x => x.Name).FirstOrDefault();
            }
        }

        // should be guaranteed not nullable by the validator
        [JsonIgnore]
        public string EnglishFlavorText
        {
            get
            {
                return FlavorTextEntries.Where(el => el?.Language?.Name is "en")?.Select(x => x.FlavorText).FirstOrDefault();
            }
        }
    }

    public class FlavorTextEntry
    {
        [JsonPropertyName("flavor_text")]
        public string FlavorText { get; set; }

        [JsonPropertyName("language")]
        public Entry Language { get; set; }

    }

    public class NameEntry
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("language")]
        public Entry Language { get; set; }
    }

    public class Entry
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

    }

    #endregion

    #region Validator

    public class PokemonDescriptionValidator : AbstractValidator<PokemonDescription>
    {
        private const string LANG_CODE = "en";
        public PokemonDescriptionValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.IsLegendary)
                .NotNull()
                .WithMessage("Legendary status not found");

            RuleFor(x => x.FlavorTextEntries)
                .NotNull()
                .WithMessage("No Description found")
                .NotEmpty()
                .WithMessage("Description is empty")
                .Must(x => x.Any(el => el.Language.Name == LANG_CODE))
                .WithMessage("No Description found for english language")
                .DependentRules(() =>
                {
                    RuleForEach(x => x.FlavorTextEntries)
                        .Where(x => x.Language.Name == LANG_CODE)
                        .Must(x => !string.IsNullOrEmpty(x.FlavorText))
                        .WithMessage("Description for english language is empty");
                });

            RuleFor(x => x.NameEntries)
                .NotNull()
                .WithMessage("No Name found")
                .NotEmpty()
                .WithMessage("Names are empty")
                .Must(x => x.Any(el => el.Language.Name == LANG_CODE))
                .WithMessage("No Name found for english language")
                .DependentRules(() =>
                {
                    RuleForEach(x => x.NameEntries)
                        .Where(x => x.Language.Name == LANG_CODE)
                        .Must(x => !string.IsNullOrEmpty(x.Name))
                        .WithMessage("Name for english language is empty");
                });


            RuleFor(x => x.Habitat)
                .NotNull()
                .WithMessage("No Habitat found")
                .Must(el => !string.IsNullOrEmpty(el?.Name))
                .WithMessage("No Habitat name found");

        }
    }

    #endregion
}
