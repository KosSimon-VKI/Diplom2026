using Microsoft.EntityFrameworkCore;
using GardenNookApi.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using GardenNookApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IPreparationStockService, PreparationStockService>();
builder.Services.Configure<PickupSchedulingOptions>(
    builder.Configuration.GetSection(PickupSchedulingOptions.SectionName));
builder.Services.AddSingleton<IPickupSchedulingService, PickupSchedulingService>();
builder.Services.Configure<KitchenPickupFilterOptions>(
    builder.Configuration.GetSection(KitchenPickupFilterOptions.SectionName));

// CORS  Web (7009) + cookies
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:7009")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "GardenNook.Auth";
        options.Cookie.HttpOnly = true;

        //   cross-origin cookie:
        //  cookie  origin  SameSite=None + Secure
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);

        //  API  401/403
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };


    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

// CORS 
app.UseCors("WebClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
