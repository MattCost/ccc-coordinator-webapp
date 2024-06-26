using System.Text.RegularExpressions;
using Azure.Data.Tables;
using CCC.Entities;
using CCC.Enums;
using CCC.Exceptions;
using CCC.Services.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CCC.Services.EntityProvider;

public class EntityProviderTableStorage : IEntityProvider
{
    private static string BikeRoutes => nameof(BikeRoutes);
    private static string GroupRides => nameof(GroupRides);
    private static string RideEvents => nameof(RideEvents);


    protected readonly ISecretsManager _secretsManager;
    private static string EntitiesTable => "Entities";
    private static string FavoritesTable => "Favorites";
    private static string IsDeletedFlag => "IsDeleted";

    protected readonly ILogger<EntityProviderTableStorage> _logger;
    protected TableClient EntitiesTableClient { get; private set; }
    protected TableClient FavoritesTableClient { get; private set; }
    public EntityProviderTableStorage(ILogger<EntityProviderTableStorage> logger, ISecretsManager secretsManager)
    {
        _logger = logger;
        _secretsManager = secretsManager;

        var connectionString = _secretsManager.GetSecret("STORAGE_ACT_CONNECTION_STRING");
        EntitiesTableClient = new TableClient(connectionString, EntitiesTable);
        FavoritesTableClient = new TableClient(connectionString, FavoritesTable);
    }

    #region PublicInterface

    #region BikeRoutes
    public async Task<BikeRoute> GetBikeRoute(Guid routeId) => await GetEntityAsync<BikeRoute>(routeId.ToString(), BikeRoutes);

    public async Task<IEnumerable<BikeRoute>> GetAllBikeRoutes() => await GetAllEntitiesAsync<BikeRoute>(BikeRoutes);
    public async Task DeleteBikeRoute(Guid routeId)
    {
        var rides = await GetRidesUsingRoute(routeId);
        if (rides.Any())
        {
            throw new EntityLockedException("Bike Route is used by Rides. Can't be deleted");
        }
        await DeleteEntityAsync(BikeRoutes, routeId.ToString());
    }

    public async Task RestoreBikeRoute(Guid routeId) => await RestoreEntityAsync(BikeRoutes, routeId.ToString());

    public async Task UpdateBikeRoute(BikeRoute bikeRoute) => await UpsertEntityAsync(bikeRoute, BikeRoutes, bikeRoute.Id.ToString());

    #endregion

    #region GroupRides
    public async Task<GroupRide> GetGroupRide(Guid rideId) => await GetEntityAsync<GroupRide>(rideId.ToString(), GroupRides);

    public async Task<IEnumerable<GroupRide>> GetAllGroupRides() => await GetAllEntitiesAsync<GroupRide>(GroupRides);

    public async Task DeleteGroupRide(Guid rideId)
    {
        _logger.LogDebug("Getting parent event id");
        var groupRide = await GetGroupRide(rideId);
        var parentEventId = groupRide.RideEventId;

        // delete ride first, then you can delete events and routes
        _logger.LogDebug("Deleting group ride id {RideId}", rideId);
        await DeleteEntityAsync(GroupRides, rideId.ToString());

        _logger.LogDebug("Getting Ride Event for parentEventId");
        var parentEvent = await GetRideEvent(parentEventId);
        if (parentEvent.RideIds.Contains(rideId))
        {
            _logger.LogDebug("ParentEvent.Rides does contain RideId. removing");
            parentEvent.RideIds.Remove(rideId);
            await UpdateRideEvent(parentEvent);
        }
    }
    public async Task RestoreGroupRide(Guid rideId) => await RestoreEntityAsync(GroupRides, rideId.ToString());

    public async Task UpdateGroupRide(GroupRide groupRide)
    {
        //try catch not found, return "invalid parent id" message
        var parentEvent = await GetRideEvent(groupRide.RideEventId);
        if (!parentEvent.RideIds.Contains(groupRide.Id))
        {
            parentEvent.RideIds.Add(groupRide.Id);
            await UpdateRideEvent(parentEvent);
        }
        //Once we know the parent Id is valid, update/create the groupRide
        await UpsertEntityAsync(groupRide, GroupRides, groupRide.Id.ToString());
    }

    #endregion

    #region RideEvents
    public async Task<RideEvent> GetRideEvent(Guid eventId) => await GetEntityAsync<RideEvent>(eventId.ToString(), RideEvents);

    public async Task<IEnumerable<RideEvent>> GetAllRideEvents() => await GetAllEntitiesAsync<RideEvent>(RideEvents);

    public async Task DeleteRideEvent(Guid eventId, bool force = false)
    {
        _logger.LogDebug("Deleting Ride Event {EventId}", eventId);
        var rides = await GetRidesAtEvent(eventId);
        _logger.LogDebug("Ride Event has {Count} rides", rides.Count());
        if (rides.Any())
        {
            if(!force)
            {
                _logger.LogWarning("Event {Id} has rides, so event can't be deleted", eventId);
                throw new EntityLockedException("Event has Group rides that must be deleted first");
            }
            _logger.LogDebug("Ride Event {EventId}. Force Delete true. Removing all rides", eventId);
            foreach(var ride in rides)
            {
                await DeleteGroupRide(ride.Id);
            }
        }
        _logger.LogDebug("Deleting Ride Event {Id}", eventId);
        await DeleteEntityAsync(RideEvents, eventId.ToString());
    }


    public async Task RestoreRideEvent(Guid eventId) => await RestoreEntityAsync(RideEvents, eventId.ToString());


    public async Task UpdateRideEvent(RideEvent rideEvent) => await UpsertEntityAsync(rideEvent, RideEvents, rideEvent.Id.ToString());

    #endregion

    public async Task<List<GroupRide>> GetRidesUsingRoute(Guid routeId)
    {
        var queryFilter = $"PartitionKey eq '{GroupRides}' and BikeRouteId eq guid'{routeId}' and not {IsDeletedFlag}";

        return await QueryHelper<GroupRide>(queryFilter);
    }

    #endregion

    #region PrivateMethods
    private static TModel CreateFromTableEntity<TModel>(TableEntity tableEntity) where TModel : class, new()
    {
        var output = new TModel();
        var properties = typeof(TModel).GetProperties();
        foreach (var property in properties)
        {
            if (!tableEntity.ContainsKey(property.Name))
                continue;

            var value = IsSupportedType(property.PropertyType) ?
                tableEntity[property.Name] :
                System.Text.Json.JsonSerializer.Deserialize(tableEntity[property.Name].ToString() ?? "{}", property.PropertyType);

            property.SetMethod?.Invoke(output, new[] { value });
            // property.SetValue(output, value);
        }
        return output;
    }
    private async Task<TModel> GetEntityAsync<TModel>(string rowKey, string partitionKey) where TModel : class, new()
    {
        _logger.LogTrace("Getting Partition:Row {Partition}:{Row} from Table {Table}", partitionKey, rowKey, EntitiesTable);
        try
        {
            var tableEntity = await EntitiesTableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
            return CreateFromTableEntity<TModel>(tableEntity);

        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, EntitiesTable);
            throw new EntityNotFoundException($"{EntitiesTable}:{partitionKey}:{rowKey} not found");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    private async Task<IEnumerable<TModel>> GetAllEntitiesAsync<TModel>(string partitionKey) where TModel : class, new()
    {
        var queryFilter = $"PartitionKey eq '{partitionKey}' and not {IsDeletedFlag}";
        return await QueryHelper<TModel>(queryFilter);
    }

    private async Task<List<GroupRide>> GetRidesAtEvent(Guid eventId)
    {
        _logger.LogDebug("Getting GroupRides at event {Id}", eventId);
        var queryFilter = $"PartitionKey eq '{GroupRides}' and RideEventId eq guid'{eventId}' and not {IsDeletedFlag}";
        return await QueryHelper<GroupRide>(queryFilter);
    }

    private async Task<List<T>> QueryHelper<T>(string queryFilter) where T : class, new()
    {
        _logger.LogDebug("Entering Query Helper. Filter:`{Filter}`", queryFilter);
        try
        {
            var output = new List<T>();
            var queryResults = EntitiesTableClient.QueryAsync<TableEntity>(filter: queryFilter);
            await foreach (var result in queryResults)
            {
                output.Add(CreateFromTableEntity<T>(result));
            }
            _logger.LogDebug("Output has {Count} entries", output.Count);
            return output;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Table {TableName} not found", EntitiesTable);
            throw new EntityNotFoundException($"{EntitiesTable} not found");
        }

        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    private static bool IsSupportedType(Type type)
    {
        if (type == typeof(string)) return true;
        if (type == typeof(Guid)) return true;
        if (type == typeof(bool)) return true;
        if (type == typeof(DateTime)) return true;
        if (type == typeof(DateTimeOffset)) return true;
        if (type == typeof(double)) return true;
        if (type == typeof(int)) return true;
        if (type == typeof(long)) return true;

        return false;
    }
    private static Dictionary<string, object> ModelToDict<TModel>(TModel model)
    {
        var dict = new Dictionary<string, object>();

        foreach (var property in typeof(TModel).GetProperties())
        {
            dict[property.Name] = IsSupportedType(property.PropertyType) ?
                property.GetValue(model) ?? new object() :
                System.Text.Json.JsonSerializer.Serialize(property.GetValue(model));
        }

        return dict;
    }

    private async Task UpsertEntityAsync<TModel>(TModel model, string partitionKey, string rowKey)
    {
        _logger.LogDebug("Entering Upsert Entity. Partition Key {Partition}. Row Key {Row}", partitionKey, rowKey);
        try
        {
            var tableEntity = new TableEntity(ModelToDict(model))
            {
                RowKey = rowKey,
                PartitionKey = partitionKey
            };
            tableEntity.Add(IsDeletedFlag, false);
            await EntitiesTableClient.UpsertEntityAsync(tableEntity);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Table store returned 404");
            throw new EntityNotFoundException("Table not found?");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    private async Task DeleteEntityAsync(string partitionKey, string rowKey)
    {
        try
        {
            await EntitiesTableClient.UpsertEntityAsync(
                new TableEntity(partitionKey, rowKey)
                {
                    [IsDeletedFlag] = true
                });
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, EntitiesTable);
            throw new EntityNotFoundException($"{EntitiesTable}:{partitionKey}:{rowKey} not found");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    private async Task RestoreEntityAsync(string partitionKey, string rowKey)
    {
        try
        {
            await EntitiesTableClient.UpsertEntityAsync(
                new TableEntity(partitionKey, rowKey)
                {
                    [IsDeletedFlag] = false
                });
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, EntitiesTable);
            throw new EntityNotFoundException($"{EntitiesTable}:{partitionKey}:{rowKey} not found");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    public async Task<List<Guid>> GetFavoriteRoutes(string userId)
    {
        _logger.LogDebug("Entering GetFavoriteRoutes. UserId {User} ", userId);
        if(string.IsNullOrEmpty(userId))
        {
            throw new Exception("UserId must be provided");
        }
        
        var queryFilter = $"PartitionKey eq '{userId}' and EntityType eq '{EntityTypes.BikeRoute.ToString()}'";
        try
        {
            var output = new List<Guid>();
            var queryResults = FavoritesTableClient.QueryAsync<TableEntity>(filter: queryFilter);
            await foreach (var result in queryResults)
            {
                if(result.ContainsKey("EntityId") && result["EntityId"] is Guid bikeRouteId)
                {
                    output.Add(bikeRouteId);
                }
            }
            _logger.LogDebug("Output has {Count} entries", output.Count);
            return output;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Table {TableName} not found", EntitiesTable);
            throw new EntityNotFoundException($"{EntitiesTable} not found");
        }

        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    public async Task AddFavoriteRoute(string userId, Guid bikeRouteId)
    {
        _logger.LogDebug("Entering AddFavoriteRoute. UserId {User} BikeRouteId {BikeRouteId}", userId, bikeRouteId);
        if(string.IsNullOrEmpty(userId))
        {
            throw new Exception("UserId must be provided");
        }
        //validate route id?
        var entry = new TableEntity(userId, bikeRouteId.ToString())
        {
            ["EntityType"] = EntityTypes.BikeRoute.ToString(),
            ["EntityId"] = bikeRouteId
        };
        await FavoritesTableClient.UpsertEntityAsync(entry);
    }

    // If we add other favorite types, this just becomes removeFavorite OR we add entity validation to this
    public async Task RemoveFavoriteRoute(string userId, Guid bikeRouteId)
    {
        _logger.LogDebug("Entering RemoveFavoriteRoute. UserId {User} BikeRouteId {BikeRouteId}", userId, bikeRouteId);
        if(string.IsNullOrEmpty(userId))
        {
            throw new Exception("UserId must be provided");
        }

        await FavoritesTableClient.DeleteEntityAsync(userId, bikeRouteId.ToString());
    }


    #endregion
}