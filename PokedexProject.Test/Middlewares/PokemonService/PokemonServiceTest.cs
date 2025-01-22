using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokedexProject.Clients.PokemonClient;
using PokedexProject.Controllers;
using PokedexProject.Models;
using Slugify;

namespace PokedexProject.Test.Middlewares.PokemonService
{

    public class PokemonServiceTest : IDisposable
    {

        #region Test Data

        public static IEnumerable<object[]> TestPokemon()
        {
            yield return new object[] { "en", "hello world", "swamp", "MewTwo", true };
            yield return new object[] { "en", "hello world", "swamp", "Nidoran\u2640", false };
            yield return new object[] { "en", "hello world", "swamp", "Farfetch'd", false };
            yield return new object[] { "en", "hello world", "swamp", "Mr. Mime", false };
            yield return new object[] { "en", "hello world", "swamp", "Flabébé", false };
            yield return new object[] { "en", "hello world", "swamp", "Mime Jr.", false };
        }

        public static IEnumerable<object[]> TestHabitat()
        {
            yield return new object[] { null };
            yield return new object[] { new Entry()
            {
                Name = null
            } };
            yield return new object[] { new Entry()
            {
                Name = ""
            } };
        }

        public static IEnumerable<object[]> TestDescription()
        {
            yield return new object[] { null };
            yield return new object[] { new List<FlavorTextEntry>() };
        }

        public static IEnumerable<object[]> TestNames()
        {
            yield return new object[] { null };
            yield return new object[] { new List<NameEntry>() };
        }

        public static IEnumerable<object[]> TestDescriptionContent()
        {
            yield return new object[]
            {
                new List<FlavorTextEntry>()
                {
                    new()
                    {
                        FlavorText = null,
                        Language = new Entry()
                        {
                            Name = "en"
                        }
                    }
                }
            };
            yield return new object[]
            {
                new List<FlavorTextEntry>()
                {
                    new()
                    {
                        FlavorText = "hello world",
                        Language = new Entry()
                        {
                            Name = "es"
                        }
                    }
                }
            };
        }

        public static IEnumerable<object[]> TestNameContent()
        {
            yield return new object[]
            {
                new List<NameEntry>()
                {
                    new()
                    {
                        Name = null,
                        Language = new Entry()
                        {
                            Name = "en"
                        }
                    }
                }
            };
            yield return new object[]
            {
                new List<NameEntry>()
                {
                    new()
                    {
                        Name = "hello world",
                        Language = new Entry()
                        {
                            Name = "es"
                        }
                    }
                }
            };
        }

        #endregion

        private Mock<IPokemonClient> _pokemonClient;
        private Mock<ISlugHelper> _slugHelper;
        private Mock<IMemoryCache> _memoryCache;
        private Mock<PokemonDescriptionValidator> _pokemonValidator;

        private PokemonDescription defaultPokemon;

        private readonly IServiceProvider _serviceProvider;

        public PokemonServiceTest()
        {
            _pokemonClient = new Mock<IPokemonClient>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDependencies();
            _serviceProvider = serviceCollection.BuildServiceProvider();

            defaultPokemon = new PokemonDescription()
            {
                IsLegendary = false,
                NameEntries =
                [
                    new NameEntry()
                    {
                        Language = new Entry()
                        {
                            Name = "en",
                        },
                        Name = "mewtwo"
                    }
                ],
                FlavorTextEntries =
                [
                    new FlavorTextEntry()
                    {
                        FlavorText = "hello world",
                        Language = new Entry()
                        {
                            Name = "en",
                        }
                    }
                ],
                Habitat = new Entry()
                {
                    Name = "swamp"
                }
            };
        }

        public void Dispose() { }

        [Theory]
        [MemberData(nameof(TestPokemon))]
        public async void GetPokemonByInfo_ShouldReturnAllFieldsCorrectly_WhenAllDataIsPassed(string englishLanguageCode, string flavorText, string habitatName, string pokemonName, bool isLegendary)
        {
            string slugifiedName = _serviceProvider.GetService<ISlugHelper>().GenerateSlug(pokemonName);

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync((string pokemonName) => new PokemonDescription()
            {
                IsLegendary = isLegendary,
                NameEntries =
                [
                    new NameEntry()
                    {
                        Language = new Entry()
                        {
                            Name = englishLanguageCode,
                        },
                        Name = slugifiedName
                    }
                ],
                FlavorTextEntries =
                [
                    new FlavorTextEntry()
                    {
                        FlavorText = flavorText,
                        Language = new Entry()
                        {
                            Name = englishLanguageCode,
                        }
                    }
                ],
                Habitat = new Entry()
                {
                    Name = habitatName,
                }
            });

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(pokemonName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, true);
            Assert.NotNull(result.Data);
            Assert.Equal(result.Data.Name, slugifiedName);
            Assert.Equal(result.Data.Description, flavorText);
            Assert.Equal(result.Data.IsLegendary, isLegendary);
            Assert.Equal(result.Data.HabitatName, habitatName);
        }

        [Fact]
        public async void GetPokemonByInfo_ShouldReturnOneError_WhenHttpRequestExceptionIsThrown()
        {
            string errorMessage = "i'm a teapot";
            HttpStatusCode code = HttpStatusCode.ExpectationFailed;

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync(
                () => throw new HttpRequestException(errorMessage, null, code));

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(defaultPokemon.EnglishName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, false);
            Assert.Null(result.Data);
            Assert.Equal(result.StatusCode, code);
            Assert.Collection(result.Errors.Errors, el =>
            {
                Assert.Equal(el.ErrorMessage, errorMessage);
            });
        }

        [Fact]
        public async void GetPokemonByInfo_ShouldReturnOneError_WhenAnyOtherExceptionIsThrown()
        {
            string errorMessage = "i'm not a teapot";

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync(
                () => throw new Exception(errorMessage));

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(defaultPokemon.EnglishName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, false);
            Assert.Null(result.Data);
            Assert.Equal(result.StatusCode, HttpStatusCode.InternalServerError);
            Assert.Collection(result.Errors.Errors, el =>
            {
                Assert.Equal(el.ErrorMessage, errorMessage);
            });
        }

        [Theory]
        [MemberData(nameof(TestHabitat))]
        public async void GetPokemonByInfo_ShouldReturnOneError_WhenHabitatIsNullOrEmpty(Entry habitat)
        {

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync(
                (string pokemonName) =>
                {
                    defaultPokemon.Habitat = habitat;
                    return defaultPokemon;
                });

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(defaultPokemon.EnglishName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, false);
            Assert.Collection(result.Errors.Errors, el =>
            {
                Assert.Equal(el.ErrorMessage, habitat == null ? "No Habitat found" : "No Habitat name found");
            });
        }

        [Theory]
        [MemberData(nameof(TestDescription))]
        public async void GetPokemonByInfo_ShouldReturnOneError_WhenFlavorEntriesIsNullOrEmpty(List<FlavorTextEntry> flavorEntries)
        {

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync(
                (string pokemonName) =>
                {
                    defaultPokemon.FlavorTextEntries = flavorEntries;
                    return defaultPokemon;
                });

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(defaultPokemon.EnglishName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, false);
            Assert.Collection(result.Errors.Errors, el =>
            {
                Assert.Equal(el.ErrorMessage, flavorEntries == null ? "No Description found" : "Description is empty");
            });
        }

        [Theory]
        [MemberData(nameof(TestDescriptionContent))]
        public async void GetPokemonByInfo_ShouldReturnOneError_WhenFlavorEntriesIsNotEnglishOrDoesntHaveDescripton(List<FlavorTextEntry> flavorEntries)
        {

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync(
                (string pokemonName) =>
                {
                    defaultPokemon.FlavorTextEntries = flavorEntries;
                    return defaultPokemon;
                });

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(defaultPokemon.EnglishName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, false);
            Assert.Collection(result.Errors.Errors, el =>
            {
                Assert.Equal(el.ErrorMessage, flavorEntries.FirstOrDefault().Language.Name == "es" ? "No Description found for english language" : "Description for english language is empty");
            });
        }

        [Theory]
        [MemberData(nameof(TestNames))]
        public async void GetPokemonByInfo_ShouldReturnOneError_WhenNamesIsNullOrEmpty(List<NameEntry> nameEntries)
        {

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync(
                (string pokemonName) =>
                {
                    defaultPokemon.NameEntries = nameEntries;
                    return defaultPokemon;
                });

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(defaultPokemon.EnglishName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, false);
            Assert.Collection(result.Errors.Errors, el =>
            {
                Assert.Equal(el.ErrorMessage, nameEntries == null ? "No Name found" : "Names are empty");
            });
        }

        [Theory]
        [MemberData(nameof(TestNameContent))]
        public async void GetPokemonByInfo_ShouldReturnOneError_WhenNameEntriesIsNotEnglishOrDoesntHaveDescripton(List<NameEntry> nameEntries)
        {

            _pokemonClient.Setup(_ => _.GetPokemonDescriptionByName(It.IsAny<string>())).ReturnsAsync(
                (string pokemonName) =>
                {
                    defaultPokemon.NameEntries = nameEntries;
                    return defaultPokemon;
                });

            var service = new PokedexProject.Middlewares.PokemonService.PokemonService(_pokemonClient.Object, _serviceProvider.GetService<ISlugHelper>(), _serviceProvider.GetService<IMemoryCache>(), _serviceProvider.GetService<PokemonDescriptionValidator>());

            var result = await service.GetPokemonByInfo(defaultPokemon.EnglishName);

            Assert.NotNull(result);
            Assert.Equal(result.Success, false);
            Assert.Collection(result.Errors.Errors, el =>
            {
                Assert.Equal(el.ErrorMessage, nameEntries.FirstOrDefault().Language.Name == "es" ? "No Name found for english language" : "Name for english language is empty");
            });
        }

    }

}
