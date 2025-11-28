namespace NotificationService.Shared.Constants;

public static class CacheConstants
{
    public const string NotificationTemplateCacheKey = "NotificationTemplates";
    public const string NotificationSettingsCacheKey = "NotificationSettings";
    public const string ProviderStatusCacheKey = "ProviderStatus";
    public const int CacheExpirationInMinutes = 30;
    public const int TemplateCacheExpirationInHours = 24;
}