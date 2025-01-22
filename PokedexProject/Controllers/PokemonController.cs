using System.Net;
using Microsoft.AspNetCore.Mvc;
using PokedexProject.Middlewares.PokemonService;
using PokedexProject.Models;
using System.Text.Json.Serialization;
using PokedexProject.Middlewares;

namespace PokedexProject.Controllers
{
    public class PokemonDisplay
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("is_legendary")]
        public bool IsLegendary { get; set; }

        [JsonPropertyName("habitat")]
        public string HabitatName { get; set; }
    }



    [Route("pokemon")]
    public class PokemonController(IPokemonService pokemonService) : ControllerBase
    {

        [HttpGet("{pokemonName}")]
        public async Task<IActionResult> Get([FromRoute] string pokemonName)
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
    }
}
