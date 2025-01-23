using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokedexProject.Clients.PokemonClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PokedexProject.Clients.TranslationClient;
using PokedexProject.Middlewares.PokemonService;
using PokedexProject.Models;
using PokedexProject.Middlewares;
using Slugify;

namespace PokedexProject.Test.Middlewares.TranslationService
{
    public class TranslationServiceTest : IDisposable
    {
        private Mock<ITranslationClient> _translationClient;
        private Mock<IPokemonService> _pokemonService;
        private readonly IServiceProvider _serviceProvider;

        private PokemonDTO defaultPokemon;
        private TranslationResponse defaultTranslation;
        public TranslationServiceTest()
        {
            _translationClient = new Mock<ITranslationClient>();
            _pokemonService = new Mock<IPokemonService>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDependencies();
            _serviceProvider = serviceCollection.BuildServiceProvider();

            defaultPokemon = new PokemonDTO()
            {
                Description = "normal Description",
                HabitatName = "swamp",
                IsLegendary = true,
                Name = "mewtwo"
            };

            defaultTranslation = new TranslationResponse()
            {
                SuccessItemCounter = new SuccessItemCounter()
                {
                    Total = 1
                },
                Contents = new Contents()
                {
                    Translated = "translated text",
                    Text = "normal Description",
                    Translation = "yoda"
                }
            };

        }
        public void Dispose() { }

        [Fact]
        public async void GetTranslatedPokemonByName_ShouldReturnYodaTranslatedPokemonDescription_WhenAllDataIsProvided()
        {
            _pokemonService.Setup(x => x.GetPokemonByName(It.IsAny<string>())).ReturnsAsync(Result<PokemonDTO>.SuccessResult(defaultPokemon));
            _translationClient.Setup(x => x.GetYodaTranslation(It.IsAny<string>())).ReturnsAsync(defaultTranslation);

            var translationService = new PokedexProject.Middlewares.TranslationService.TranslationService(
                _translationClient.Object, _serviceProvider.GetService<ISlugHelper>(),
                _serviceProvider.GetService<IMemoryCache>(),
                _serviceProvider.GetService<TranslationResponseValidator>(), _pokemonService.Object);

            var result = await translationService.GetTranslatedPokemonByName(defaultPokemon.Name);

            Assert.True(result.Success);
            Assert.Equal(defaultTranslation.Contents.Translated, result.Data.Description);
        }

        [Fact]
        public async void GetTranslatedPokemonByName_ShouldReturnShakeSpearTranslatedPokemonDescription_WhenAllDataIsProvided()
        {
            _pokemonService.Setup(x => x.GetPokemonByName(It.IsAny<string>())).ReturnsAsync(() =>
            {
                defaultPokemon.IsLegendary = false;
                return Result<PokemonDTO>.SuccessResult(defaultPokemon);
            });
            _translationClient.Setup(x => x.GetShakespeareTranslation(It.IsAny<string>())).ReturnsAsync(defaultTranslation);

            var translationService = new PokedexProject.Middlewares.TranslationService.TranslationService(
                _translationClient.Object, _serviceProvider.GetService<ISlugHelper>(),
                _serviceProvider.GetService<IMemoryCache>(),
                _serviceProvider.GetService<TranslationResponseValidator>(), _pokemonService.Object);

            var result = await translationService.GetTranslatedPokemonByName(defaultPokemon.Name);

            Assert.True(result.Success);
            Assert.Equal(defaultTranslation.Contents.Translated, result.Data.Description);
        }

        [Fact]
        public async void GetTranslatedPokemonByName_ShouldReturnDefaultTranslatedPokemonDescription_WhenTranslationThrowsException()
        {
            _pokemonService.Setup(x => x.GetPokemonByName(It.IsAny<string>())).ReturnsAsync(() =>
            {
                defaultPokemon.IsLegendary = false;
                return Result<PokemonDTO>.SuccessResult(defaultPokemon);
            });
            _translationClient.Setup(x => x.GetShakespeareTranslation(It.IsAny<string>())).ReturnsAsync(() =>
            {
                throw new HttpRequestException("i'm a teapot", null, HttpStatusCode.ExpectationFailed);
            });

            var translationService = new PokedexProject.Middlewares.TranslationService.TranslationService(
                _translationClient.Object, _serviceProvider.GetService<ISlugHelper>(),
                _serviceProvider.GetService<IMemoryCache>(),
                _serviceProvider.GetService<TranslationResponseValidator>(), _pokemonService.Object);

            var result = await translationService.GetTranslatedPokemonByName(defaultPokemon.Name);

            Assert.True(result.Success);
            Assert.Equal(defaultPokemon.Description, result.Data.Description);
        }

        [Fact]
        public async void GetTranslatedPokemonByName_ShouldReturnDefaultTranslatedPokemonDescription_WhenTranslationReturnInvalidResponse()
        {
            _pokemonService.Setup(x => x.GetPokemonByName(It.IsAny<string>())).ReturnsAsync(() =>
            {
                defaultPokemon.IsLegendary = true;
                return Result<PokemonDTO>.SuccessResult(defaultPokemon);
            });
            _translationClient.Setup(x => x.GetYodaTranslation(It.IsAny<string>())).ReturnsAsync(() =>
            {
                defaultTranslation.SuccessItemCounter.Total = 0;
                return defaultTranslation;
            });

            var translationService = new PokedexProject.Middlewares.TranslationService.TranslationService(
                _translationClient.Object, _serviceProvider.GetService<ISlugHelper>(),
                _serviceProvider.GetService<IMemoryCache>(),
                _serviceProvider.GetService<TranslationResponseValidator>(), _pokemonService.Object);

            var result = await translationService.GetTranslatedPokemonByName(defaultPokemon.Name);

            Assert.True(result.Success);
            Assert.Equal(defaultPokemon.Description, result.Data.Description);
        }

    }
}
