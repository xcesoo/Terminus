using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Terminus.Application;
using Terminus.Domain.Interfaces;
using Terminus.Domain.Services;
using Terminus.Infrastructure;
using Terminus.Infrastructure.Extensions;
using Terminus.WebApi.Middlewares;
using Terminus.WebApi.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionsHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Terminus API",
        Version = "v1",
        Description = "API терміналу логістичної компанії: договори, вироби, споживачі, ТТН та друковані форми (PDF)."
    });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlFile))
        options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);

    options.SchemaFilter<DateTimeExampleSchemaFilter>();
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddSingleton<IDocumentNumberGenerator, DocumentNumberGenerator>();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
