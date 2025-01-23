using PokedexProject.Controllers;
using PokedexProject.Models;
using static PokedexProject.Middlewares.PokemonService.PokemonService;

namespace PokedexProject.Middlewares.PokemonService
{
    public interface IPokemonService
    {
        public Task<Result<PokemonDTO>> GetPokemonByName(string pokemonName);
    }
}
