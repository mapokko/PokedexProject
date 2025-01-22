using System.Net;
using Microsoft.Extensions.Caching.Memory;
using PokedexProject.Clients.PokemonClient;
using PokedexProject.Models;
using System.Text.Json.Serialization;
using PokedexProject.Controllers;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using Slugify;

namespace PokedexProject.Middlewares.PokemonService
{
    public class PokemonService(IPokemonClient pokemonClient, ISlugHelper slug, IMemoryCache memoryCache, PokemonDescriptionValidator validator) : IPokemonService
    {

        public async Task<Result<PokemonDisplay>> GetPokemonByInfo(string pokemonName)
        {
            string slugifiedPokemonName = slug.GenerateSlug(pokemonName);
            if(memoryCache.TryGetValue(slugifiedPokemonName, out PokemonDisplay cachedPokemon))
            {
                return Result<PokemonDisplay>.SuccessResult(cachedPokemon);
            }

            PokemonDescription pokemonDescription = null;
            try
            {
                pokemonDescription = await pokemonClient.GetPokemonDescriptionByName(slugifiedPokemonName);
            }
            catch (HttpRequestException ex)
            {
                return Result<PokemonDisplay>.ErrorResult(ex.StatusCode ?? HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return Result<PokemonDisplay>.ErrorResult(HttpStatusCode.InternalServerError, ex.Message);

            }

            ValidationResult validationResult = validator.Validate(pokemonDescription);

            if (!validationResult.IsValid)
            {
                return Result<PokemonDisplay>.ErrorResult(HttpStatusCode.NotFound, validationResult.Errors.Select(el => el.ErrorMessage).ToList());
            }

            PokemonDisplay result = new PokemonDisplay
            {
                Name = pokemonDescription.EnglishName,
                Description = Regex.Replace(pokemonDescription.EnglishFlavorText, @"\s+", " "),
                IsLegendary = pokemonDescription.IsLegendary,
                HabitatName = pokemonDescription.Habitat.Name
            };

            memoryCache.Set(slugifiedPokemonName, result);

            return Result<PokemonDisplay>.SuccessResult(result); ;
        }
    }
}
