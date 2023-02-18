using System;
using System.Collections.Generic;
using System.Linq;
using OpenTelemetry.Resources;

public static class ResourceBuilderExtensions
{
    /// <summary>
    /// Adds any values it finds from the AppService environment variables
    /// <see href="https://docs.microsoft.com/en-us/azure/app-service/reference-app-settings">AppService Environment Variables</see>
    /// </summary>
    /// <param name="resourceBuilder"></param>
    /// <returns></returns>
    public static ResourceBuilder AddAzureAttributes(this ResourceBuilder resourceBuilder)
    {
        var envVars = Environment.GetEnvironmentVariables();
        var attributesToAdd = new List<KeyValuePair<string, object>>();
 
        var envVarsToAdd = new List<Tuple<string, string>> {
            new("azure.appservice.site_name", "WEBSITE_SITE_NAME"),
            new("azure.resource_group", "WEBSITE_RESOURCE_GROUP"),
            new("azure.subscription_id", "WEBSITE_OWNER_NAME"),
            new("azure.region", "REGION_NAME"),
            new("azure.appservice.platform_version", "WEBSITE_PLATFORM_VERSION"),
            new("azure.appservice.sku", "WEBSITE_SKU"),
            new("azure.appservice.bitness", "SITE_BITNESS"), // x86 vs AMD64
            new("azure.appservice.hostname", "WEBSITE_HOSTNAME"),
            new("azure.appservice.role_instance_id", "WEBSITE_ROLE_INSTANCE_ID"),
            new("azure.appservice.slot_name", "WEBSITE_SLOT_NAME"),
            new("azure.appservice.instance_id", "WEBSITE_INSTANCE_ID"),
            new("azure.appservice.website_logging_enabled", "WEBSITE_HTTPLOGGING_ENABLED"),
            new("azure.appservice.internal_ip", "WEBSITE_PRIVATE_IP"),
            new("azure.appservice.functions_extensions_version", "FUNCTIONS_EXTENSION_VERSION"),
            new("azure.appservice.functions.worker_runtime", "FUNCTIONS_WORKER_RUNTIME"),
            new("azure.appservice.function_placeholder_mode", "WEBSITE_PLACEHOLDER_MODE"),
        };
 
        resourceBuilder.AddAttributes(
            envVarsToAdd
                .Where(attr => envVars.Contains(attr.Item2) && 
                       !string.IsNullOrEmpty(envVars[attr.Item2].ToString()))
                .Select(attr =>
                {
                    var (name, key) = attr;
                    return new KeyValuePair<string, object>(name, envVars[key].ToString());
                })
        );
         
        resourceBuilder.AddAttributes(attributesToAdd);
        return resourceBuilder;
    }
}