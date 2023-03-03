

using DatacontextTest2;
using IdentityTest2.Email;
using IdentityTest2.Entities;
using IdentityTest2.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(
);

/*builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});*/

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    /*opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;*/
})
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false; // for development only
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:SecretKey"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
});


/*builder.Services.AddControllers();*/


builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddDbContext<SchoolContext>();
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<SchoolContext>()
    .AddSignInManager<SignInManager<AppUser>>()
    .AddDefaultTokenProviders();


/*app.UseHttpsRedirection();*/
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Jwt Authorization",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
     {
     {
     new OpenApiSecurityScheme
     {
     Reference =new OpenApiReference
     {
     Type=ReferenceType.SecurityScheme,
     Id = "Bearer"
     }
     },new string[]
     {}
     }
     });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
        await next.Invoke();
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();





app.MapControllers();



/*using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    *//*var context = services.GetRequiredService<DataContext>();*//*
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    *//*await context.Database.MigrateAsync();*//*

}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}*/

app.Run();
