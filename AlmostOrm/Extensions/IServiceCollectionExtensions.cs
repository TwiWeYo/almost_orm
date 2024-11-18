using Microsoft.Extensions.DependencyInjection;
using ORMini.Mappers;
using ORMini.Savers;

namespace ORMini.Extensions;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMapOptions(this IServiceCollection services, MapOptions mapOptions, string? name = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (mapOptions == null)
            throw new ArgumentNullException(nameof(mapOptions));

        if (string.IsNullOrEmpty(name))
            return services.AddTransient<MapOptions>(_ => mapOptions);

        return services.AddKeyedTransient<MapOptions>(name, (_,_) => mapOptions);
    }

    public static IServiceCollection AddMapFactory(this IServiceCollection services,
                                                   Func<string, string> defaultTablePath,
                                                   Func<string, string> defaultProcedurePath,
                                                   string? mapOptionsName = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services
            .AddTransient<IMapSaver, MapSaver>()
            .AddTransient<Gennie>();


        if (string.IsNullOrEmpty(mapOptionsName))
            return services.AddSingleton<MapFactory>(q =>
                new MapFactory(q.GetRequiredService<MapOptions>()!, q.GetRequiredService<Gennie>()!)
                {
                    DefaultTablePath = defaultTablePath,
                    DefaultProcedurePath = defaultProcedurePath
                });

        return services.AddSingleton<MapFactory>(q =>
                new MapFactory(q.GetKeyedService<MapOptions>(mapOptionsName)!, q.GetService<Gennie>()!)
                {
                    DefaultTablePath = defaultTablePath,
                    DefaultProcedurePath = defaultProcedurePath
                });
    }
}
