using System.Net.Http.Headers;
using ArchiTrackerBE.Data;
using ArchiTrackerBE.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=App_Data/architracker.db";
    options.UseSqlite(connectionString);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<ArchipelagoTrackerService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("ArchiTracker/1.0");
});

var app = builder.Build();

var dataDirectory = Path.Combine(app.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    const string swaggerHtml = """
        <!DOCTYPE html>
        <html lang="en">
          <head>
            <meta charset="UTF-8" />
            <title>ArchiTracker API</title>
            <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
            <style>
              body { margin: 0; }
              #swagger-ui { height: 100vh; }
            </style>
          </head>
          <body>
            <div id="swagger-ui"></div>
            <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js" crossorigin></script>
            <script>
              window.onload = () => {
                SwaggerUIBundle({
                  url: '/openapi/v1.json',
                  dom_id: '#swagger-ui',
                  docExpansion: 'none',
                });
              };
            </script>
          </body>
        </html>
        """;

    app.MapGet("/swagger", () => Results.Text(swaggerHtml, "text/html")).ExcludeFromDescription();
    app.MapGet("/swagger/index.html", () => Results.Text(swaggerHtml, "text/html")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
