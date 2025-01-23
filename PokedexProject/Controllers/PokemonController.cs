using System.Net;
using Microsoft.AspNetCore.Mvc;
using PokedexProject.Middlewares.PokemonService;
using PokedexProject.Models;
using System.Text.Json.Serialization;
using PokedexProject.Middlewares;
using PokedexProject.Middlewares.TranslationService;

namespace PokedexProject.Controllers
{
    [Route("pokemon")]
    public class PokemonController(IPokemonService pokemonService, ITranslationService translationService) : ControllerBase
    {
        /// <summary>
        /// Searches information about the provided pokemon name.
        /// </summary>
        /// <param name="pokemonName">The name of the pokemon</param>
        /// <returns>A newly created TodoItem</returns>
        /// <remarks>
        /// Sample response:
        ///
        ///     {
        ///         "description": "Spits fire that is hot enough to melt boulders. Known to cause forest fires unintentionally.",
        ///         "habitat": "mountain",
        ///         "is_legendary": false,
        ///         "name": "Charizard"
        ///     }
        ///
        /// Sample Error:
        /// 
        ///     {
        ///         "error_list":
        ///         [
        ///             {
        ///                 "error_message": "Not Found"
        ///             }
        ///         ]
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns the information about the pokemon</response>
        /// <response code="400">If the pokemon name is missing</response>
        /// <response code="404">If the item is not found or any of the 4 required attributes are not found</response>
        /// <response code="500">For any internal error</response>
        [HttpGet("{pokemonName}")]
        public async Task<IActionResult> GetPokemon([FromRoute] string pokemonName)
        {
            if (pokemonName is null or "")
            {
                var errorList = Result<object>.ErrorResult(HttpStatusCode.BadRequest, ["Pokemon name cannot be empty"]);
                return new ObjectResult(errorList.Errors) { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            var result = await pokemonService.GetPokemonByName(pokemonName);

            if (result.Success)
                return Ok(result.Data);

            var objectResult = new ObjectResult(result.Errors)
            {
                StatusCode = (int)result.StatusCode,
            };

            return objectResult;
        }

        /// <summary>
        /// Searches information about the provided pokemon name and translates the description using funtranslation's yoda or shakespeare apis'.
        /// </summary>
        /// <param name="pokemonName">The name of the pokemon</param>
        /// <returns>A newly created TodoItem</returns>
        /// <remarks>
        /// Sample response:
        ///
        ///     {
        ///         "name": "Charizard",
        ///         "description": "Spits fire yond is hot enow to melt boulders. Known to cause forest fires unintentionally.",
        ///         "is_legendary": false,
        ///         "habitat": "mountain"
        ///     }
        ///
        /// Sample Error:
        /// 
        ///     {
        ///         "error_list":
        ///         [
        ///             {
        ///                 "error_message": "Not Found"
        ///             }
        ///         ]
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns the information about the pokemon alongside the translated description</response>
        /// <response code="400">If the pokemon name is missing</response>
        /// <response code="404">If the item is not found or any of the 4 required attributes are not found</response>
        /// <response code="500">For any internal error</response>

        [HttpGet("translated/{pokemonName}")]
        public async Task<IActionResult> GetTranslatedPokemon([FromRoute] string pokemonName)
        {
            if (pokemonName is null or "")
            {
                var errorList = Result<object>.ErrorResult(HttpStatusCode.BadRequest, ["Pokemon name cannot be empty"]);
                return new ObjectResult(errorList.Errors) { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            var result = await translationService.GetTranslatedPokemonByName(pokemonName);

            if (result.Success)
                return Ok(result.Data);

            var objectResult = new ObjectResult(result.Errors)
            {
                StatusCode = (int)result.StatusCode,
            };

            return objectResult;
        }
    }
}
