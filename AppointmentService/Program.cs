using AppointmentService.Data;
using AppointmentService.Middleware;
using AppointmentService.Repositories;
using AppointmentService.Security;
using AppointmentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "The 'Jwt' section is missing from configuration.");

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppointmentDb")));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            RoleClaimType = "role",
            NameClaimType = "name",
        };

    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Auth Service v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
 
app.UseAuthorization();

app.MapControllers();

app.Run();
