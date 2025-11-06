using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Debug configuration loading
Console.WriteLine("Current Environment: " + builder.Environment.EnvironmentName);
Console.WriteLine("Configuration Sources:");
foreach (var source in ((IConfigurationRoot)builder.Configuration).Providers)
{
    Console.WriteLine($"- {source.GetType().Name}");
}


var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("Connection String: " + (connStr ?? "NULL"));

if (string.IsNullOrEmpty(connStr))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. " +
        "Please check appsettings.json and ensure it's being copied to output directory.");
}
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Add services to the container
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var key = "ThisIsASecretKeyForJWT";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();
var app = builder.Build();
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapGet("/", () => "Hello World!");

app.MapGet("/todos", async (ToDoDbContext db) => await db.Items.ToListAsync());

app.MapPost("/todos", async (ToDoDbContext db, Item todo) =>
{
    db.Items.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPut("/todos/{id}", async (ToDoDbContext db, int id, Item updatedTodo) =>
{
    var todo = await db.Items.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Name = updatedTodo.Name;
    todo.IsComplete = updatedTodo.IsComplete;

    await db.SaveChangesAsync();
    return Results.Ok(todo);
});

app.MapDelete("/todos/{id}", async (ToDoDbContext db, int id) =>
{
    var todo = await db.Items.FindAsync(id);
    if (todo is null) return Results.NotFound();

    db.Items.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapPost("/register", async (ToDoDbContext db, TodoApi.User user) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(user);
});

app.MapPost("/login", async (ToDoDbContext db, TodoApi.LoginDto login) =>
{
    var user = (await db.Users.ToListAsync()).FirstOrDefault(u =>
    {
        var type = u.GetType();
        var unameProp = type.GetProperty("Username") ?? type.GetProperty("UserName") ?? type.GetProperty("Name");
        var pwdProp = type.GetProperty("Password");
        var uname = unameProp?.GetValue(u)?.ToString();
        var pwd = pwdProp?.GetValue(u)?.ToString();
        return uname == login.Username && pwd == login.Password;
    });

    if (user == null) return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenKey = Encoding.UTF8.GetBytes(key);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, login.Username) }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);
    return Results.Ok(new { token = jwt });
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();

// ---------------------------------------------------
// Definitions moved here or inside namespaces
namespace TodoApi
{
    public record LoginDto(string Username, string Password);

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
