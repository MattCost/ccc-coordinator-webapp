namespace CCC.Entities;

public class User
{
    public string UserId {get;set;} = string.Empty;
    public string DisplayName {get;set;} = string.Empty;
    public IDictionary<string, object> AdditionalData {get;set;} = new Dictionary<string, object>();
}