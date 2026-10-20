using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Configuration;

public sealed record AuthServiceSettings(
    string ConnectionString,
    string AllowedHosts,
    string JwtSecretKey,
    string JwtIssuer,
    string JwtAudience,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays,
    int PasswordRequiredLength,
    bool PasswordRequireDigit,
    bool PasswordRequireNonAlphanumeric,
    bool PasswordRequireUppercase,
    bool PasswordRequireLowercase,
    bool RequireUniqueEmail,
    bool RequireConfirmedEmail,
    bool RegistrationEnabled)
{
    public static AuthServiceSettings Load(IConfiguration configuration)
    {
        var connectionString = Required(configuration, "ConnectionStrings:DefaultConnection");
        var allowedHosts = Required(configuration, "AllowedHosts");
        var secretKey = Required(configuration, "Jwt:SecretKey");
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
            allowedHosts.Split(';', StringSplitOptions.TrimEntries)
                .Any(host => host == "*"))
        {
            throw new InvalidOperationException(
                "Configuration 'AllowedHosts' must not contain '*' in Production.");
        }

        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
        {
            throw new InvalidOperationException(
                "Configuration 'Jwt:SecretKey' must contain at least 32 UTF-8 bytes.");
        }

        return new AuthServiceSettings(
            connectionString,
            allowedHosts,
            secretKey,
            Required(configuration, "Jwt:Issuer"),
            Required(configuration, "Jwt:Audience"),
            PositiveInt(configuration, "Jwt:AccessTokenExpirationMinutes"),
            PositiveInt(configuration, "Jwt:RefreshTokenExpirationDays"),
            BoundedInt(configuration, "Identity:Password:RequiredLength", minimum: 8, maximum: 100),
            RequiredBool(configuration, "Identity:Password:RequireDigit"),
            RequiredBool(configuration, "Identity:Password:RequireNonAlphanumeric"),
            RequiredBool(configuration, "Identity:Password:RequireUppercase"),
            RequiredBool(configuration, "Identity:Password:RequireLowercase"),
            RequiredBool(configuration, "Identity:User:RequireUniqueEmail"),
            RequiredBool(configuration, "Identity:SignIn:RequireConfirmedEmail"),
            OptionalBool(configuration, "Features:RegistrationEnabled", defaultValue: true));
    }

    private static string Required(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Required configuration '{key}' is missing.");
    }

    private static int PositiveInt(IConfiguration configuration, string key)
    {
        var value = Required(configuration, key);
        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) &&
            result > 0)
        {
            return result;
        }

        throw new InvalidOperationException(
            $"Configuration '{key}' must be a positive whole number.");
    }

    private static int BoundedInt(
        IConfiguration configuration,
        string key,
        int minimum,
        int maximum)
    {
        var value = PositiveInt(configuration, key);
        if (value >= minimum && value <= maximum)
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Configuration '{key}' must be between {minimum} and {maximum}.");
    }

    private static bool RequiredBool(IConfiguration configuration, string key)
    {
        var value = Required(configuration, key);
        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        throw new InvalidOperationException(
            $"Configuration '{key}' must be either 'true' or 'false'.");
    }

    private static bool OptionalBool(
        IConfiguration configuration,
        string key,
        bool defaultValue)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        throw new InvalidOperationException(
            $"Configuration '{key}' must be either 'true' or 'false'.");
    }
}
