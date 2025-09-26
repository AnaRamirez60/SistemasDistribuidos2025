using System.ServiceModel;
using PokedexApi.Infrastructure.Soap.Contracts;
using PokedexApi.Mappers;
using PokedexApi.Models;
using PokedexApi.Exceptions;
using PokedexApi.Dtos;
using PokedexApi.Infrastructure.Soap.Dtos;


namespace PokedexApi.Gateways;

public class PokemonGateway : IPokemonGateway
{
    private readonly IPokemonContract _pokemonContract;
    private readonly ILogger<PokemonGateway> _logger;

    public PokemonGateway(IConfiguration configuration, ILogger<PokemonGateway> logger)
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(uri: configuration.GetValue<string>(key: "PokemonService:Url"));
        _pokemonContract = new ChannelFactory<IPokemonContract>(binding, endpoint).CreateChannel();
        _logger = logger;
    }

    public async Task<PagedResponse<Pokemon>> GetPokemonsAsync(string name, string type, int pageSize, int pageNumber, string orderBy, string orderDirection, CancellationToken cancellationToken)
    {
        var query = new Query
        {
            Name = string.Empty, 
            Type = string.Empty, 
            PageSize = 100, 
            PageNumber = 1,
            OrderBy = orderBy,
            OrderDirection = orderDirection
        };

        var paginated = await _pokemonContract.GetPokemons(query, cancellationToken);
        var pagedResponse = paginated.ToPagedResponse();

        var filteredData = pagedResponse.Data.Where(p => 
            (string.IsNullOrEmpty(name) || p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(type) || p.Type.Contains(type, StringComparison.OrdinalIgnoreCase))
        );

        var totalFilteredRecords = filteredData.Count();
        var totalPages = (int)Math.Ceiling((double)totalFilteredRecords / pageSize);
        var paginatedData = filteredData
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResponse<Pokemon>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalFilteredRecords,
            TotalPages = totalPages,
            Data = paginatedData
        };
    }


    public async Task UpdatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        try
        {
            await _pokemonContract.UpdatePokemon(pokemon.ToUpdateRequest(), cancellationToken);
        }
        catch (FaultException ex) when (ex.Message == "Pokemon not found")
        {
            throw new PokemonNotFoundException(pokemon.Id);
        }
        catch (FaultException ex) when (ex.Message == "Another pokemon with the same name already exists")
        {
            throw new PokemonAlreadyExistsException(pokemon.Name);
        }
    }

    public async Task DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _pokemonContract.DeletePokemon(id, cancellationToken);
        }
        catch (FaultException ex) when (ex.Message == "Pokemon not found")
        {
            _logger.LogWarning(ex, "Pokemon not found");
            throw new PokemonNotFoundException(id);
        }
    }

    public async Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pokemon = await _pokemonContract.GetPokemonById(id, cancellationToken);
            return pokemon.ToModel();
        }
        catch (FaultException ex) when (ex.Message == "Pokemon not found")
        {
            _logger.LogWarning(ex, message: "Pokemon not found");
            throw new PokemonNotFoundException(id);
        }
    }

    public async Task<IList<Pokemon>> GetPokemonsByNameAsync(string name, CancellationToken cancellationToken)
    {
        _logger.LogDebug(":(");
        var pokemons = await _pokemonContract.GetPokemonsByName(name, cancellationToken);
        return pokemons.ToModel();
    }

     public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending request to soap api, with pokemon: {name}", pokemon.Name);
            var createdPokemon = await _pokemonContract.CreatePokemon(pokemon.ToRequest(), cancellationToken);
            return createdPokemon.ToModel();
        }
        catch (Exception e)
        {
            _logger.LogError(e, message: "Algo trono en el create pokemon a soap");
            throw;
        }
    }
}