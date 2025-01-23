using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PokedexProject.Controllers;
using PokedexProject.Middlewares;
using PokedexProject.Middlewares.PokemonService;
using PokedexProject.Middlewares.TranslationService;

namespace PokedexProject.Test.Controllers
{
    public class PokemonControllerTest : IDisposable
    {
        private Mock<IPokemonService> _pokemonService;
        private Mock<ITranslationService> _translationService;
        private PokemonDTO defaultPokemon;

        public PokemonControllerTest()
        {
            _pokemonService = new Mock<IPokemonService>();
            _translationService = new Mock<ITranslationService>();

            defaultPokemon = new PokemonDTO()
            {
                Description = "normal Description",
                HabitatName = "swamp",
                IsLegendary = true,
                Name = "mewtwo"
            };
        }

        public void Dispose()
        {
        }

        [Fact]
        public async void GetPokemon_ShouldReturnBadRequest_WhenNoPokemonGiven()
        {
            var pokemonController = new PokemonController(_pokemonService.Object, _translationService.Object);

            var result = await pokemonController.GetPokemon("") as ObjectResult;

            Assert.NotNull(result?.Value);
            Assert.Equal(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);

            var errorList = result.Value as ErrorList;

            Assert.Collection(errorList.Errors, error =>
            {
                Assert.Equal(error.ErrorMessage, "Pokemon name cannot be empty");
            });


        }

        [Fact]
        public async void GetTranslatedPokemon_ShouldReturnBadRequest_WhenNoPokemonGiven()
        {
            var pokemonController = new PokemonController(_pokemonService.Object, _translationService.Object);
            var result = await pokemonController.GetTranslatedPokemon("") as ObjectResult;

            Assert.NotNull(result?.Value);
            Assert.Equal(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);

            var errorList = result.Value as ErrorList;

            Assert.Collection(errorList.Errors, error =>
            {
                Assert.Equal(error.ErrorMessage, "Pokemon name cannot be empty");
            });

        }

        [Fact]
        public async void GetPokemon_ShouldReturnSuccess_WhenServiceMethodSucceeds()
        {
            _pokemonService.Setup(x => x.GetPokemonByName(It.IsAny<string>())).ReturnsAsync(Result<PokemonDTO>.SuccessResult(defaultPokemon));
            var pokemonController = new PokemonController(_pokemonService.Object, _translationService.Object);

            var result = await pokemonController.GetPokemon("test") as OkObjectResult;

            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);
            Assert.NotNull(result.Value);

            var value = result.Value as PokemonDTO;
            Assert.Equal(defaultPokemon.Description, value.Description);
        }

        [Fact]
        public async void GetTranslatedPokemon_ShouldReturnSuccess_WhenServiceMethodSucceeds()
        {
            _translationService.Setup(x => x.GetTranslatedPokemonByName(It.IsAny<string>())).ReturnsAsync(Result<PokemonDTO>.SuccessResult(defaultPokemon));
            var pokemonController = new PokemonController(_pokemonService.Object, _translationService.Object);

            var result = await pokemonController.GetTranslatedPokemon("test") as OkObjectResult;

            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);
            Assert.NotNull(result.Value);

            var value = result.Value as PokemonDTO;
            Assert.Equal(defaultPokemon.Description, value.Description);
        }

        [Fact]
        public async void GetPokemon_ShouldReturnError_WhenServiceMethodGivesError()
        {
            _pokemonService.Setup(x => x.GetPokemonByName(It.IsAny<string>())).ReturnsAsync(Result<PokemonDTO>.ErrorResult(HttpStatusCode.InternalServerError, "Internal Server Error :)"));
            var pokemonController = new PokemonController(_pokemonService.Object, _translationService.Object);

            var result = await pokemonController.GetPokemon("test") as ObjectResult;

            Assert.Equal(HttpStatusCode.InternalServerError, (HttpStatusCode)result.StatusCode);

            var errorList = result.Value as ErrorList;

            Assert.Collection(errorList.Errors, error =>
            {
                Assert.Equal(error.ErrorMessage, "Internal Server Error :)");
            });

        }

        [Fact]
        public async void GetTranslatedPokemon_ShouldReturnError_WhenServiceMethodGivesError()
        {
            _translationService.Setup(x => x.GetTranslatedPokemonByName(It.IsAny<string>())).ReturnsAsync(Result<PokemonDTO>.ErrorResult(HttpStatusCode.InternalServerError, "Internal Server Error :)"));
            var pokemonController = new PokemonController(_pokemonService.Object, _translationService.Object);

            var result = await pokemonController.GetTranslatedPokemon("test") as ObjectResult;

            Assert.Equal(HttpStatusCode.InternalServerError, (HttpStatusCode)result.StatusCode);

            var errorList = result.Value as ErrorList;

            Assert.Collection(errorList.Errors, error =>
            {
                Assert.Equal(error.ErrorMessage, "Internal Server Error :)");
            });

        }
    }
}
