using PokedexProject.Models;

namespace PokedexProject.Clients.PokemonClient
{
    public interface IPokemonClient
    {

        public Task<PokemonBase> GetPokemonByName(string slugifiedPokemonName);

        public Task<PokemonDescription> GetPokemonDescriptionByName(string pokemonName);
    }
}
