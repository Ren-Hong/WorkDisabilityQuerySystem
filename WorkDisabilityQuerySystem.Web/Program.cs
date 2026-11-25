using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using WorkDisabilityQuerySystem.Application.Abstractions.Repositories;
using WorkDisabilityQuerySystem.Application.Abstractions.Security;
using WorkDisabilityQuerySystem.Application.Abstractions.Services;
using WorkDisabilityQuerySystem.Application.Services;
using WorkDisabilityQuerySystem.Domain.Accounts;
using WorkDisabilityQuerySystem.Infrastructure.Repositories;
using WorkDisabilityQuerySystem.Infrastructure.Security;
using WorkDisabilityQuerySystem.Web.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IPasswordVerifier, BCryptPasswordVerifier>();
builder.Services.AddScoped<IAccountRepository>(_ =>
    new AccountRepository(builder.Configuration.GetConnectionString("WdqsDatabase")));
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPermanentDisabilityRepository>(_ =>
    new PermanentDisabilityRepository(builder.Configuration.GetConnectionString("WdqsDatabase")));
builder.Services.AddScoped<IPermanentDisabilityService, PermanentDisabilityService>();
builder.Services.AddScoped<AccountCookieEvents>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "WorkDisabilityQuerySystem.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.EventsType = typeof(AccountCookieEvents);
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BackendAccess", policy => policy.RequireRole(Account.AdministratorLevel));
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("Login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

if (args is ["create-account", var username, var role])
{
    var level = role switch
    {
        "admin" => Account.AdministratorLevel,
        "user" => Account.UserLevel,
        _ => throw new ArgumentException("角色參數須為 admin 或 user。")
    };
    Console.Write("請輸入新帳號密碼：");
    var password = ReadPassword();
    Console.WriteLine();
    await using var scope = app.Services.CreateAsyncScope();
    var accounts = scope.ServiceProvider.GetRequiredService<IAccountService>();
    var created = await accounts.CreateAsync(username, password, level, null, CancellationToken.None);
    Console.WriteLine(created ? "帳號已建立。" : "帳號名稱已存在。");
    Environment.ExitCode = created ? 0 : 1;
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets().AllowAnonymous();
app.MapControllers();
app.MapAreaControllerRoute("frontend", "Frontend", "Frontend/{controller=Home}/{action=Index}/{id?}");
app.MapAreaControllerRoute("backend", "Backend", "Backend/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();

static string ReadPassword()
{
    if (Console.IsInputRedirected)
    {
        return Console.ReadLine() ?? string.Empty;
    }

    var password = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            return password.ToString();
        }

        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Length--;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }
}

