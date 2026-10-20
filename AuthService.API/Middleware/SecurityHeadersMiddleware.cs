namespace AuthService.API.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly IWebHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // X-Content-Type-Options: nosniff
        headers.XContentTypeOptions = "nosniff";

        // X-Frame-Options: DENY
        headers.XFrameOptions = "DENY";

        // X-XSS-Protection: 1; mode=block
        headers.XXSSProtection = "1; mode=block";

        // Referrer-Policy: strict-origin-when-cross-origin
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions-Policy: disable unwanted features
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        // Content-Security-Policy
        if (!_env.IsDevelopment() || !context.Request.Path.StartsWithSegments("/swagger"))
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
        else
            // Relaxed CSP for Swagger in development
            headers.ContentSecurityPolicy = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:;";


        // HSTS (only in production over HTTPS)
        if (!_env.IsDevelopment() && context.Request.IsHttps)
            headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains";


        await _next(context);
    }
}