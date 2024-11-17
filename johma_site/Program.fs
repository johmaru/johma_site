namespace johma_site

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
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
    
module Program =
    let exitCode = 0
        
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
 
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
    
        builder.Services.AddControllers() |> ignore
        builder
            .Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()

        builder.Services.AddRazorPages()

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error")
            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

        app.UseHttpsRedirection()

        app.UseStaticFiles()
        app.UseRouting()
        app.UseAuthorization()

        app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}")

        app.MapRazorPages()

        app.Run()

        exitCode