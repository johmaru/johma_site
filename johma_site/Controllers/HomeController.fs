namespace johma_site.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open johma_site.Models

type HomeController (logger : ILogger<HomeController>) =
    inherit Controller()

    member this.Index () =
        let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"] 
        
                        else
                                let newCookie = "Light"
                                let cookieOptions = CookieOptions()
                                cookieOptions.Expires <- DateTimeOffset.UtcNow.AddDays(30.0)
                                this.Response.Cookies.Append("Theme", newCookie, cookieOptions)
                                newCookie
                              
        this.ViewData.["Theme"] <- cookie
        this.View()

    member this.Profile () =
        let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"]  else "Light"
        this.ViewData.["Theme"] <- cookie
        this.View()
        
    member this.ThemeChange() =
        let cookie = this.Request.Cookies.["Theme"]
        let newCookie = if cookie = "Light" then "Dark" else "Light"
        let cookieOptions = CookieOptions()
        cookieOptions.Expires <- DateTimeOffset.UtcNow.AddDays(30.0)
        this.Response.Cookies.Append("Theme", newCookie, cookieOptions)
        this.RedirectToAction("Index")
        
                  

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })