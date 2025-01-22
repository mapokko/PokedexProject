using PokedexProject.Models;
using RestSharp;

namespace PokedexProject.Clients.TranslationClient
{
    public interface ITranslationClient
    {

        public Task<TranslationResponse> GetYodaTranslation(string text);

        public Task<TranslationResponse> GetShakespeareTranslation(string text);
    }
}
