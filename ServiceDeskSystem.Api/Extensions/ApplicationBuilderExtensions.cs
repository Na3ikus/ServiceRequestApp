using Serilog;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Api.Middleware;

namespace ServiceDeskSystem.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        bool hasJwtKey = !string.IsNullOrWhiteSpace(app.Configuration["Jwt:Key"]);

        if (app.Environment.IsDevelopment() && hasJwtKey)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceDesk API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/health/smtp", async (IEmailSender emailSender, CancellationToken cancellationToken) =>
        {
            var (isSuccess, message) = await emailSender.CheckConnectionAsync(cancellationToken).ConfigureAwait(false);
            return Results.Ok(new { IsAvailable = isSuccess, Message = message });
        }).AllowAnonymous();

        return app;
    }
}
