using AutoMapper;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Infraestrutura;
using Infraestrutura.Contexto;
using Infraestrutura.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using Service.Implementacoes;
using Service.Mappings;
using SistemaGestaoDeAssinatura.Components;
using System.Data;
using System.Globalization;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddServerSideBlazor()
        .AddCircuitOptions(options =>
        {
            options.DetailedErrors = true;
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(0);
        });
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    
    var handler = new HttpClientHandler();
    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
    
    // Adicionar cookies de autenticação se disponíveis
    if (httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Cookie"))
    {
        var cookieHeader = httpContextAccessor.HttpContext.Request.Headers["Cookie"].ToString();
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
        }
    }
    
    return httpClient;
});
builder.Services.AddRadzenComponents();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

ILoggerFactory logger = LoggerFactory.Create(builder => builder.AddConsole());
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new UsuarioProfile());
});
IMapper mapper = config.CreateMapper();

builder.Services.AddSingleton(mapper);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Tokens.AuthenticatorTokenProvider = "Authenticator";
}).AddRoles<Role>()
.AddRoleManager<RoleManager<Role>>()
.AddSignInManager<SignInManager<Usuario>>()
.AddRoleValidator<RoleValidator<Role>>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";
    options.AccessDeniedPath = "/";
    options.ReturnUrlParameter = "redirectUrl";
});

builder.Services.AddAuthorization(options =>
{
    // Policy para autentica��o tempor�ria (primeiro acesso)
    options.AddPolicy("TemporaryAuth", policy =>
    {
        policy.RequireClaim("TemporaryAuth", "true");
    });

    // Policy para autentica��o completa
    options.AddPolicy("FullAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            !context.User.HasClaim("TemporaryAuth", "true"));
    });
});
var app = builder.Build();


//Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

var defaultCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = new DatabaseSeeder(
            services.GetRequiredService<AppDbContext>(),
            services.GetRequiredService<UserManager<Usuario>>(),
            services.GetRequiredService<RoleManager<Role>>(),
            services.GetRequiredService<ILogger<DatabaseSeeder>>());

        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger2 = services.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "Erro ao executar seed do banco de dados");
    }
}
app.UseDeveloperExceptionPage();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

//app.MapBlazorHub();
//app.MapFallbackToPage("/_Host");
//app.Run();
