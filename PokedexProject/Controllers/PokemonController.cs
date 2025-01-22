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

        [HttpGet("{pokemonName}")]
        public async Task<IActionResult> GetPokemon([FromRoute] string pokemonName)
        {
            if (pokemonName is null or "")
            {
                var errorList = Result<object>.ErrorResult(HttpStatusCode.BadRequest, ["Pokemon name cannot be empty"]);
                return new ObjectResult(errorList.Errors) { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            var result = await pokemonService.GetPokemonByInfo(pokemonName);

            if (result.Success)
                return Ok(result.Data);

            var objectResult = new ObjectResult(result.Errors)
            {
                StatusCode = (int)result.StatusCode,
            };

            return objectResult;
        }

        [HttpGet("translated/{pokemonName}")]
        public async Task<IActionResult> GetTranslatedPokemon([FromRoute] string pokemonName)
        {
            if (pokemonName is null or "")
            {
                var errorList = Result<object>.ErrorResult(HttpStatusCode.BadRequest, ["Pokemon name cannot be empty"]);
                return new ObjectResult(errorList.Errors) { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            var result = await translationService.GetTranslatedPokemonByInfo(pokemonName);

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
