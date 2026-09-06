using Qbs.Infrastructure.DependencyInjection;
using Qbs.Infrastructure.Processing;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddStudio(builder.Configuration, false);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JobProcessor>();
builder.Services.AddHostedService<ProcessingService>();
await builder.Build().RunAsync();
