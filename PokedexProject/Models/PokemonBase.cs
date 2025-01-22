using FluentValidation;
using System.Text.Json.Serialization;

namespace PokedexProject.Models
{
    public class PokemonBase
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        //public string Description { get; set; }
        //public string Habitat { get; set; }
        //public bool IsLegendary { get; set; }
    }

    public class PokemonValidator : AbstractValidator<PokemonBase>
    {
        public PokemonValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEqual(0);
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }
}
