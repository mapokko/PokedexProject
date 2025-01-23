using Microsoft.Extensions.Caching.Memory;
using PokedexProject.Clients.PokemonClient;
using PokedexProject.Clients.TranslationClient;
using PokedexProject.Controllers;
using PokedexProject.Middlewares.PokemonService;
using PokedexProject.Models;
using Slugify;
using System.Net;
using FluentValidation.Results;

namespace PokedexProject.Middlewares.TranslationService
{
    public sealed class TranslationService(ITranslationClient translationClient, ISlugHelper slug, IMemoryCache memoryCache, TranslationResponseValidator validator, IPokemonService pokemonService) : ITranslationService
    {
        public async Task<Result<PokemonDTO>> GetTranslatedPokemonByName(string pokemonName)
        {
            if (string.IsNullOrEmpty(pokemonName.Trim()))
                Result<PokemonDTO>.ErrorResult(HttpStatusCode.BadRequest, "Pokemon name cannot be empty");

            string slugifiedPokemonName = slug.GenerateSlug(pokemonName);

            if (memoryCache.TryGetValue(slugifiedPokemonName, out PokemonCache cachedPokemon))
            {
                if (cachedPokemon.UseYoda &&  !string.IsNullOrEmpty(cachedPokemon.YodaTranslation))
                    return Result<PokemonDTO>.SuccessResult(new PokemonDTO(cachedPokemon, TranslationType.Yoda));

                if (!string.IsNullOrEmpty(cachedPokemon.ShakespeareTranslation))
                    return Result<PokemonDTO>.SuccessResult(new PokemonDTO(cachedPokemon, TranslationType.Shakespeare));
            }

            PokemonDTO pokemonDisplay = null;

            if (cachedPokemon != null)
            {
                pokemonDisplay = new PokemonDTO(cachedPokemon, TranslationType.Regular);
            }
            else
            {
                var searchPokemonResult = await pokemonService.GetPokemonByName(pokemonName);

                if (!searchPokemonResult.Success)
                    return searchPokemonResult;

                pokemonDisplay = searchPokemonResult.Data;
            }

            TranslationResponse translationResponse = null;
            try
            {
                if (pokemonDisplay.UseYoda)
                    translationResponse = await translationClient.GetYodaTranslation(pokemonDisplay.Description);
                else
                    translationResponse = await translationClient.GetShakespeareTranslation(pokemonDisplay.Description);
            }
            catch (Exception ex)
            {
                return Result<PokemonDTO>.SuccessResult(pokemonDisplay);
            }

            ValidationResult validationResult = validator.Validate(translationResponse);

            if (!validationResult.IsValid)
            {
                return Result<PokemonDTO>.SuccessResult(pokemonDisplay);
            }

            if (cachedPokemon == null)
                cachedPokemon = new PokemonCache(pokemonDisplay);


            if (pokemonDisplay.UseYoda)
            {
                cachedPokemon.YodaTranslation = translationResponse.Contents.Translated;
            }
            else
            {
                cachedPokemon.ShakespeareTranslation = translationResponse.Contents.Translated;
            }

            pokemonDisplay.Description = translationResponse.Contents.Translated;
            memoryCache.Set(slugifiedPokemonName, cachedPokemon);

            return Result<PokemonDTO>.SuccessResult(pokemonDisplay);
        }

    }
}
