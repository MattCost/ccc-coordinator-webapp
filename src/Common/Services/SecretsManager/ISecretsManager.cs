namespace CCC.Services.Secrets;

public interface ISecretsManager
{
    string GetSecret(string secretName);
}