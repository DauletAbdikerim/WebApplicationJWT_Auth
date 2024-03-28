using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using WebApplicationJWT_Auth;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();

string connection = "Server=(localdb)\\mssqllocaldb;Database=jwtdb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

string secretKey = "this is my custom Secret key for authentication";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    string loginForm = @"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>Login Form</title>
    </head>
    <body>
        <h2>Login Form</h2>
        <form method='post'>
            <p>
                <label>Email</label><br />
                <input name='email' />
            </p>
            <p>
                <label>Password</label><br />
                <input type='password' name='password' />
            </p>
            <input type='submit' value='Login' />
        </form>
    </body>
    </html>";
    await context.Response.WriteAsync(loginForm);
});

app.MapPost("/login", async (HttpContext context, ApplicationContext dbContext) =>
{
    var form = await context.Request.ReadFormAsync();
    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
        return Results.BadRequest("Email and/or password are not provided");

    string email = form["email"];
    string password = form["password"];

    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
    if (user == null)
        return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, user.Email)
        }),
        Expires = DateTime.UtcNow.AddHours(1), 
        SigningCredentials = creds
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok($"{user.Email} is authenticated with JWT token!!!");
});

app.MapGet("/logout", async (HttpContext context) =>
{
    return Results.Redirect("/login");
});

app.Map("/", [Authorize] () => $"Hello World!");

app.Run();