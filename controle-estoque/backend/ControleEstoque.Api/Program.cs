using ControleEstoque.Api.Infrastructure;
using ControleEstoque.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<EstoqueDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddExceptionHandler<ErroHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(o => o.AddPolicy("Angular", p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<EstoqueDbContext>().Database.EnsureCreated();

app.UseExceptionHandler();
app.UseCors("Angular");
app.MapControllers();
app.Run();