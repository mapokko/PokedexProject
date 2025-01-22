using PokedexProject.Models;

namespace PokedexProject.Clients.PokemonClient
{
    public interface IPokemonClient
    {

        public Task<PokemonDescription> GetPokemonDescriptionByName(string pokemonName);
    }
}
