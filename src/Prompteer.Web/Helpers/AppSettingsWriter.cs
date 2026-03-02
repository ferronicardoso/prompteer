using System.Text.Json.Nodes;

namespace Prompteer.Web.Helpers;

/// <summary>Writes AzureAd section into appsettings.json at runtime, preserving all other keys.</summary>
public static class AppSettingsWriter
{
    public static void WriteAzureAd(
        string appSettingsPath,
        string tenantId,
        string clientId,
        string? clientSecret,
        string? domain)
    {
        var json = File.Exists(appSettingsPath)
            ? File.ReadAllText(appSettingsPath)
            : "{}";

        var root = JsonNode.Parse(json)!.AsObject();

        if (root["AzureAd"] is not JsonObject azureAd)
        {
            azureAd = new JsonObject();
            root["AzureAd"] = azureAd;
        }

        azureAd["Instance"]     = "https://login.microsoftonline.com/";
        azureAd["TenantId"]     = tenantId;
        azureAd["ClientId"]     = clientId;
        azureAd["CallbackPath"] = "/signin-oidc";

        // Only overwrite Domain if a value was provided
        if (!string.IsNullOrWhiteSpace(domain))
            azureAd["Domain"] = domain;
        else if (azureAd["Domain"] == null)
            azureAd["Domain"] = "";

        // Only overwrite ClientSecret if a new value was provided; preserve existing otherwise
        if (!string.IsNullOrWhiteSpace(clientSecret))
            azureAd["ClientSecret"] = clientSecret;
        else if (azureAd["ClientSecret"] == null)
            azureAd["ClientSecret"] = "";

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(appSettingsPath, root.ToJsonString(options));
    }
}
