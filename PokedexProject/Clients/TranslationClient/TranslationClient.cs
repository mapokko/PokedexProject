using System.Text.Json;
using PokedexProject.Models;
using RestSharp;

namespace PokedexProject.Clients.TranslationClient
{
    public class TranslationClient() :  ITranslationClient 
    {
        private readonly RestClient _client = new("https://api.funtranslations.com/translate/");

        public async Task<TranslationResponse> GetYodaTranslation(string text)
        {
            return await GetTranslation(text, true);
        }

        public async Task<TranslationResponse> GetShakespeareTranslation(string text)
        {
            return await GetTranslation(text, false);
        }


        private async Task<TranslationResponse> GetTranslation(string text, bool useYoda)
        {
            var request = new RestRequest(useYoda ? "yoda" : "shakespeare" , Method.Post);
            request.AddParameter("text", text);

            var response = await _client.ExecuteAsync<TranslationResponse>(request);

            if (!response.IsSuccessful)
            {
                var error = JsonSerializer.Deserialize<TranslationError>(response.Content);
                throw new HttpRequestException(error.Error.Message, response.ErrorException, error.Error.Code);
            }

            return response.Data;
        }


    }
}
