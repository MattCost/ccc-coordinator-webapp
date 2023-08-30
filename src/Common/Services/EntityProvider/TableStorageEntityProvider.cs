using Azure.Data.Tables;
using CCC.Entities;
using CCC.Exceptions;
using CCC.Services.Secrets;
using Microsoft.Extensions.Logging;

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
    public EntityProviderTableStorage(ILogger<EntityProviderTableStorage> logger, ISecretsManager secretsManager)
    {
        _logger = logger;
        _secretsManager = secretsManager;

        var connectionString = _secretsManager.GetSecret("STORAGE_ACT_CONNECTION_STRING");
        TableClient = new TableClient(connectionString, TableName);

        _secretsManager = secretsManager;
    }

    #region PublicInterface

    public async Task<BikeRoute> GetBikeRoute(Guid routeId) => await GetEntityAsync<BikeRoute>(routeId.ToString(), BikeRoutes);

    public async Task<IEnumerable<BikeRoute>> GetAllBikeRoutes() => await GetAllEntitiesAsync<BikeRoute>(BikeRoutes);
    public async Task DeleteBikeRoute(Guid routeId)
    {
        //todo check if route is used first
        await DeleteEntityAsync(BikeRoutes, routeId.ToString());
    }
    public async Task RestoreBikeRoute(Guid routeId) => await RestoreEntityAsync(BikeRoutes, routeId.ToString());

    public async Task UpdateBikeRoute(BikeRoute bikeRoute) => await UpsertEntityAsync(bikeRoute, BikeRoutes, bikeRoute.Id.ToString());

    public async Task<GroupRide> GetGroupRide(Guid rideId) => await GetEntityAsync<GroupRide>(rideId.ToString(), GroupRides);

    public async Task<IEnumerable<GroupRide>> GetAllGroupRides() => await GetAllEntitiesAsync<GroupRide>(GroupRides);

    public async Task DeleteGroupRide(Guid rideId)
    {
        // delete ride first, then you can delete events and routes
        await DeleteEntityAsync(GroupRides, rideId.ToString());
        var parentEvent = await GetRideEvent(rideId);
        if(parentEvent.Rides.Contains(rideId))
        {
            parentEvent.Rides.Remove(rideId);
            await UpdateRideEvent(parentEvent);
        }
    }
    public async Task RestoreGroupRide(Guid rideId) => await RestoreEntityAsync(GroupRides, rideId.ToString());

    public async Task UpdateGroupRide(GroupRide groupRide)
    {
        await UpsertEntityAsync(groupRide, GroupRides, groupRide.Id.ToString());
        var parentEvent = await GetRideEvent(groupRide.RideEventId);
        if(!parentEvent.Rides.Contains(groupRide.Id))
        {
            parentEvent.Rides.Add(groupRide.Id);
            await UpdateRideEvent(parentEvent);
        }
    }
    public async Task<RideEvent> GetRideEvent(Guid eventId) => await GetEntityAsync<RideEvent>(eventId.ToString(), RideEvents);

    public async Task<IEnumerable<RideEvent>> GetAllRideEvents() => await GetAllEntitiesAsync<RideEvent>(RideEvents);

    public async Task DeleteRideEvent(Guid eventId)
    {
        await DeleteEntityAsync(RideEvents, eventId.ToString());
    }
    public async Task RestoreRideEvent(Guid eventId) => await RestoreEntityAsync(RideEvents, eventId.ToString());


    public async Task UpdateRideEvent(RideEvent rideEvent) => await UpsertEntityAsync(rideEvent, RideEvents, rideEvent.Id.ToString());

    public Task<IEnumerable<Coordinator>> GetCoordinators()
    {
        throw new NotImplementedException();
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
        try
        {
            await Task.CompletedTask;
            var queryFilter = $"PartitionKey eq '{partitionKey}' and not IsDeleted";
            var queryResults = TableClient.QueryAsync<TableEntity>(filter: queryFilter).ToBlockingEnumerable();
            var output = queryResults.Select(CreateFromTableEntity<TModel>).ToList();
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