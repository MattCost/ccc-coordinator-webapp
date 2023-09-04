using System.Text.RegularExpressions;
using Azure.Data.Tables;
using CCC.Entities;
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
    private static string TableName => "Entities";

    protected readonly ILogger<EntityProviderTableStorage> _logger;
    protected TableClient TableClient { get; private set; }
    private readonly GraphServiceClient _graphServiceClient;
    public EntityProviderTableStorage(ILogger<EntityProviderTableStorage> logger, ISecretsManager secretsManager, GraphServiceClient graphServiceClient)
    {
        _logger = logger;
        _secretsManager = secretsManager;

        var connectionString = _secretsManager.GetSecret("STORAGE_ACT_CONNECTION_STRING");
        TableClient = new TableClient(connectionString, TableName);

        _secretsManager = secretsManager;
        _graphServiceClient = graphServiceClient;
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
        if (parentEvent.Rides.Contains(rideId))
        {
            _logger.LogDebug("ParentEvent.Rides does contain RideId. removing");
            parentEvent.Rides.Remove(rideId);
            await UpdateRideEvent(parentEvent);
        }
    }
    public async Task RestoreGroupRide(Guid rideId) => await RestoreEntityAsync(GroupRides, rideId.ToString());

    public async Task UpdateGroupRide(GroupRide groupRide)
    {
        //try catch not found, return "invalid parent id" message
        var parentEvent = await GetRideEvent(groupRide.RideEventId);
        if (!parentEvent.Rides.Contains(groupRide.Id))
        {
            parentEvent.Rides.Add(groupRide.Id);
            await UpdateRideEvent(parentEvent);
        }
        //Once we know the parent Id is valid, update/create the groupRide
        await UpsertEntityAsync(groupRide, GroupRides, groupRide.Id.ToString());
    }

    #endregion

    #region RideEvents
    public async Task<RideEvent> GetRideEvent(Guid eventId) => await GetEntityAsync<RideEvent>(eventId.ToString(), RideEvents);

    public async Task<IEnumerable<RideEvent>> GetAllRideEvents() => await GetAllEntitiesAsync<RideEvent>(RideEvents);

    public async Task DeleteRideEvent(Guid eventId)
    {
        _logger.LogDebug("Deleting Ride Event {EventId}", eventId);
        var rides = await GetRidesAtEvent(eventId);
        _logger.LogDebug("Ride Event has {Count} rides", rides.Count());
        if (rides.Any())
        {
            _logger.LogWarning("Event {Id} has rides, so event can't be deleted", eventId);
            throw new EntityLockedException("Event has Group rides that must be deleted first");
        }
        _logger.LogDebug("Deleting Ride Event {Id}", eventId);
        await DeleteEntityAsync(RideEvents, eventId.ToString());
    }


    public async Task RestoreRideEvent(Guid eventId) => await RestoreEntityAsync(RideEvents, eventId.ToString());


    public async Task UpdateRideEvent(RideEvent rideEvent) => await UpsertEntityAsync(rideEvent, RideEvents, rideEvent.Id.ToString());

    #endregion

    #region  Coordinators

    //Will I get these from MSGraph api?
    public async Task<IEnumerable<Coordinator>> GetCoordinators()
    {
        var users = await _graphServiceClient.Users.GetAsync();
        if(users == null || users.Value == null) throw new Exception("cant get users");
        return users.Value.Select( user => new Coordinator{ DisplayName = user.DisplayName ?? "mystery", UserId = user.Id ?? user.UserPrincipalName ?? "fuck me",});
        // var x = await _graphServiceClient.Users.Request().Select( u => u.Id).GetAsync();
        // return x.Select( x => new Coordinator {DisplayName = x.DisplayName, UserId = x.Id}).ToList();
    }

    public Task DeleteCoordinator(Guid coordinatorId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCoordinator(Coordinator coordinator)
    {
        throw new NotImplementedException();
    }
    #endregion

    #endregion

    #region PrivateMethods

    private async Task<List<RideEvent>> GetRidesUsingRoute(Guid routeId)
    {
        var queryFilter = $"PartitionKey eq '{GroupRides}' and BikeRouteId eq guid'{routeId}' and not IsDeleted";

        return await QueryHelper<RideEvent>(queryFilter);
    }
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

            property.SetValue(output, value);
        }
        return output;
    }
    private async Task<TModel> GetEntityAsync<TModel>(string rowKey, string partitionKey) where TModel : class, new()
    {
        _logger.LogTrace("Getting Partition:Row {Partition}:{Row} from Table {Table}", partitionKey, rowKey, TableName);
        try
        {
            var tableEntity = await TableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
            return CreateFromTableEntity<TModel>(tableEntity);

        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, TableName);
            throw new EntityNotFoundException($"{TableName}:{partitionKey}:{rowKey} not found");
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
        var queryFilter = $"PartitionKey eq '{partitionKey}' and not IsDeleted";
        return await QueryHelper<TModel>(queryFilter);
        // try
        // {
        //     await Task.CompletedTask;
        //     var queryFilter = $"PartitionKey eq '{partitionKey}' and not IsDeleted";
        //     var queryResults = TableClient.QueryAsync<TableEntity>(filter: queryFilter).ToBlockingEnumerable();
        //     var output = queryResults.Select(CreateFromTableEntity<TModel>).ToList();
        //     return output;
        // }
        // catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        // {
        //     _logger.LogWarning("Table {TableName} not found", TableName);
        //     throw new EntityNotFoundException($"{TableName} not found");
        // }

        // catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        // {
        //     _logger.LogError(ex, "Bad Request generated");
        //     throw;
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Exception");
        //     throw;
        // }
    }

    private async Task<List<GroupRide>> GetRidesAtEvent(Guid eventId)
    {
        _logger.LogDebug("Getting GroupRides at event {Id}", eventId);
        var queryFilter = $"PartitionKey eq '{GroupRides}' and RideEventId eq guid'{eventId}' and not IsDeleted";
        return await QueryHelper<GroupRide>(queryFilter);
    }

    private async Task<List<T>> QueryHelper<T>(string queryFilter) where T : class, new()
    {
        _logger.LogDebug("Entering Query Helper. Filter:`{Filter}`", queryFilter);
        try
        {
            var output = new List<T>();
            var queryResults = TableClient.QueryAsync<TableEntity>(filter: queryFilter);
            await foreach (var result in queryResults)
            {
                output.Add(CreateFromTableEntity<T>(result));
            }
            _logger.LogDebug("Output has {Count} entries", output.Count);
            return output;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Table {TableName} not found", TableName);
            throw new EntityNotFoundException($"{TableName} not found");
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
            tableEntity.Add("IsDeleted", false);
            await TableClient.UpsertEntityAsync(tableEntity);
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
            await TableClient.UpsertEntityAsync(
                new TableEntity(partitionKey, rowKey)
                {
                    ["IsDeleted"] = true
                });
            // await TableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, TableName);
            throw new EntityNotFoundException($"{TableName}:{partitionKey}:{rowKey} not found");
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
            await TableClient.UpsertEntityAsync(
                new TableEntity(partitionKey, rowKey)
                {
                    ["IsDeleted"] = false
                });
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, TableName);
            throw new EntityNotFoundException($"{TableName}:{partitionKey}:{rowKey} not found");
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


    #endregion
}