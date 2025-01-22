using FluentValidation;
using PokedexProject.Clients.PokemonClient;
using PokedexProject.Clients.TranslationClient;
using PokedexProject.Middlewares.PokemonService;
using PokedexProject.Middlewares.TranslationService;
using PokedexProject.Models;
using Slugify;

namespace PokedexProject
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddValidatorsFromAssemblyContaining<PokemonDescriptionValidator>();
            services.AddSingleton<IPokemonClient, PokemonClient>();
            services.AddSingleton<ITranslationClient, TranslationClient>();
            services.AddScoped<IPokemonService, PokemonService>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddSingleton<ISlugHelper>(_ =>
            {
                var options = new SlugHelperConfiguration();
                options.StringReplacements.Add("\u2640", "-f");
                options.StringReplacements.Add("\u2642", "-m");
                options.StringReplacements.Add(".", "");

                return new SlugHelper(options);
            });
            return services;
        }
    }
}
