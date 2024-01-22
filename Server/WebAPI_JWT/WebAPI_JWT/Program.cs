using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI_JWT.Data;
using WebAPI_JWT.Extensions;
using WebAPI_JWT.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOwnSwaggerService();


builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddDbContext<DataContext>(
   options => options.UseInMemoryDatabase("AppDb"));

builder.Services.AddOwnIdentityService(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true)
               .AllowCredentials());

app.UseExceptionHandler(opt => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
