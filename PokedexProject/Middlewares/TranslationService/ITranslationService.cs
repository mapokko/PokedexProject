using PokedexProject.Controllers;

namespace PokedexProject.Middlewares.TranslationService
{
    public interface ITranslationService
    {
        public Task<Result<PokemonDisplay>> GetTranslatedPokemonByInfo(string pokemonName);
    }
}
