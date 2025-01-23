using PokedexProject.Models;

namespace PokedexProject.Clients.PokemonClient
{
    public interface IPokemonClient
    {
		/// <summary>
		/// Get info on pokemon based on its slugified name
		/// </summary>
		/// <param name="slugifiedPokemonName">Name of the pokemon in SlugCase</param>
		/// <returns>The response form pokeapi.</returns>
		/// <exception cref="HttpRequestException">Thrown when request towards PokeApi is not successfull</exception>
		public Task<PokemonDescription> GetPokemonDescriptionByName(string pokemonName);
    }
}
