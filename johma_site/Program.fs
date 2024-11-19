namespace johma_site

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.DataProtection
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore
open DataLibrary
        
        
[<ApiController>]
[<Route("api/[controller]")>]
type UsersController(db: ApplicationDbContext) =
    inherit ControllerBase()

type Role = 
    | Admin = 1
    | User = 2

    
module Program =
    let exitCode = 0
        
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
        
        
        builder.Services.AddDataProtection()
                .SetApplicationName("johma_site")
                |> ignore
                
        builder.Services.ConfigureApplicationCookie(fun options ->
            options.Cookie.HttpOnly <- true
            options.Cookie.SecurePolicy <- CookieSecurePolicy.Always
            options.ExpireTimeSpan <- TimeSpan.FromDays(30.0)
            options.SlidingExpiration <- true
            ) |> ignore
 
       // 構成の設定
        builder.Configuration
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
            |> ignore
            
        // DbContextの設定
        builder.Services.AddDbContext<ApplicationDbContext>(fun options ->
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")
            ) |> ignore
        ) |> ignore
        
        builder.Services.AddAuthentication(fun (options: AuthenticationOptions) -> 
            options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(fun options ->
            options.ExpireTimeSpan <- TimeSpan.FromMinutes(30.0)
            options.SlidingExpiration <- true
            options.AccessDeniedPath <- PathString("/Home/Login")
            options.LoginPath <- PathString("/Home/Login")) |> ignore
    
        builder.Services.AddSession() |> ignore
        
        builder.Services.AddControllers() |> ignore
        builder
            .Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()

        builder.Services.AddRazorPages()

        let app = builder.Build()
        
        use scope = app.Services.CreateScope()
        let db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
        if not (db.Users.Any(fun u -> u.Role <> null)) then
             let users = db.Users.ToList()
             for user in users do
               user.Role <- "User"

        let adminUser = db.Users.Find(1)
        if not (isNull adminUser) then
          adminUser.Role <- "Admin"
          db.SaveChanges() |> ignore

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error")
            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

        app.UseHttpsRedirection()

        app.UseStaticFiles()
        app.UseRouting()
        app.UseAuthentication()
        app.UseAuthorization()
        
        app.UseSession() |> ignore

        app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}")

        app.MapRazorPages()

        app.Run()

        exitCode