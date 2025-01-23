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
    public sealed class PokemonService(IPokemonClient pokemonClient, ISlugHelper slug, IMemoryCache memoryCache, PokemonDescriptionValidator validator) : IPokemonService
    {

        public async Task<Result<PokemonDTO>> GetPokemonByName(string pokemonName)
        {
            if(string.IsNullOrEmpty(pokemonName.Trim()))
                Result<PokemonDTO>.ErrorResult(HttpStatusCode.BadRequest, "Pokemon name cannot be empty");

            string slugifiedPokemonName = slug.GenerateSlug(pokemonName);
            if(memoryCache.TryGetValue(slugifiedPokemonName, out PokemonCache cachedPokemon))
            {
                return Result<PokemonDTO>.SuccessResult(new PokemonDTO(cachedPokemon, TranslationType.Regular));
            }

            PokemonDescription pokemonDescription = null;
            try
            {
                pokemonDescription = await pokemonClient.GetPokemonDescriptionByName(slugifiedPokemonName);
            }
            catch (HttpRequestException ex)
            {
                return Result<PokemonDTO>.ErrorResult(ex.StatusCode ?? HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return Result<PokemonDTO>.ErrorResult(HttpStatusCode.InternalServerError, ex.Message);

            }

            ValidationResult validationResult = validator.Validate(pokemonDescription);

            if (!validationResult.IsValid)
            {
                return Result<PokemonDTO>.ErrorResult(HttpStatusCode.NotFound, validationResult.Errors.Select(el => el.ErrorMessage).ToList());
            }

            PokemonDTO result = new PokemonDTO
            {
                Name = pokemonDescription.EnglishName,
                Description = Regex.Replace(pokemonDescription.EnglishFlavorText, @"\s+", " "),
                IsLegendary = pokemonDescription.IsLegendary,
                HabitatName = pokemonDescription.Habitat.Name
            };

            memoryCache.Set(slugifiedPokemonName, new PokemonCache(result));

            return Result<PokemonDTO>.SuccessResult(result);
        }
    }
}
