using ControleEstoque.Api.Infrastructure;
using ControleEstoque.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(); // junto dos outros builder.Services
builder.Services.AddControllers();
builder.Services.AddDbContext<EstoqueDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddExceptionHandler<ErroHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(o => o.AddPolicy("Angular", p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // com: using Scalar.AspNetCore;
}
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<EstoqueDbContext>().Database.EnsureCreated();

app.UseExceptionHandler();
app.UseCors("Angular");
app.MapControllers();
app.Run();
