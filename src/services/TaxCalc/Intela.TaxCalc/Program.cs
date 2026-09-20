using Intela.TaxCalc.Managers;
using Intela.TaxCalc.Providers;
using Intela.TaxCalc.Strategy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton<ITaxStrategy, FederalTaxStrategy>();
builder.Services.AddSingleton<ICalculationProvider, InMemoryCalculationProvider>();
builder.Services.AddScoped<TaxCalculationManager>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
