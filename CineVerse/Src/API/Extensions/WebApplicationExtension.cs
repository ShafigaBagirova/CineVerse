namespace API.Extensions;

public static class WebApplicationExtension
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        ConfigurePipelineAsync(app).GetAwaiter().GetResult();
        return app;
    }
    public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}