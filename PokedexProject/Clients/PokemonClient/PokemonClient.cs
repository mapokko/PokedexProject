using PokedexProject.Models;
using RestSharp;

namespace PokedexProject.Clients.PokemonClient
{
    public class PokemonClient() : IPokemonClient 
    {
        private readonly RestClient _client = new("https://pokeapi.co/api/v2/");

        /// <summary>
        /// Get info on pokemon based on its slugified name
        /// </summary>
        /// <param name="slugifiedPokemonName">Name of the pokemon in SlugCase</param>
        /// <returns>The response form pokeapi.</returns>
        /// <exception cref="HttpRequestException">Thrown when request towards PokeApi is not successfull</exception>
        public async Task<PokemonDescription> GetPokemonDescriptionByName(string slugifiedPokemonName)
        {
            var request = new RestRequest($"pokemon-species/{slugifiedPokemonName}");

            var response = await _client.ExecuteAsync<PokemonDescription>(request);

            if (!response.IsSuccessful)
            {
                throw new HttpRequestException(response.ErrorMessage ?? response.Content, response.ErrorException, response.StatusCode);
            }

            return response.Data;
        }
    }
}
