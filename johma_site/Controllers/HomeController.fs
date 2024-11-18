namespace johma_site.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Security.Claims
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open johma_site.Models
open johma_site
open DataLibrary
open Microsoft.EntityFrameworkCore
type HomeController (logger : ILogger<HomeController>, db: ApplicationDbContext) =
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
        
    member this.Register () =
        let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"]  else "Light"
        this.ViewData.["Theme"] <- cookie
        this.View()
        
    member this.UsrProfile() =
        let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"]  else "Light"
        this.ViewData.["Theme"] <- cookie
        
        let userIdCalaim = this.User.FindFirst(ClaimTypes.NameIdentifier)
        if isNull userIdCalaim then
            this.RedirectToAction("Login") :> ActionResult
        else
            let userId = userIdCalaim.Value
            let user = db.Users.FirstOrDefault(fun x -> x.Id = int userId)
            if isNull user then
                this.RedirectToAction("Login") :> ActionResult
            else
                this.View(user) :> ActionResult
                
    member this.Logout() =
        async {
            do! this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme) |> Async.AwaitTask
            return this.RedirectToAction("Index")
             } |> Async.StartAsTask             
        
    member this.RegisterAcc(name: String,email: String, password: String,password_confirm: String) : ActionResult =
      let trimmedPassword = password.Trim()
      let trimmedPasswordConfirm = password_confirm.Trim()
      let mutable hasError = false
      
      if trimmedPassword <> trimmedPasswordConfirm then
        this.ModelState.AddModelError("Password", "パスワードが一致しません")
        hasError <- true

      let existUser = db.Users.FirstOrDefault(fun x -> x.Email = email)
      if not (isNull existUser) then
         this.ModelState.AddModelError("Email", "このメールアドレスは既に登録されています")
         hasError <- true

      let existName = db.Users.FirstOrDefault(fun x -> x.Name = name)
      if not (isNull existName) then
        this.ModelState.AddModelError("Name", "このユーザー名は既に登録されています")
        hasError <- true

      if hasError then
          let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"] else "Light"
          this.ViewData.["Theme"] <- cookie
          this.View("Register") :> ActionResult
      else
        let newUser = User(Name = name, Email = email, Password = trimmedPassword)
        db.Users.Add(newUser) |> ignore
        db.SaveChanges() |> ignore
        this.RedirectToAction("Login") :> ActionResult
        
        
    member this.LoginAcc(email: String,password: String) : ActionResult =
        let trimmedPassword = password.Trim()
        let existUser = db.Users.FirstOrDefault(fun x -> x.Email = email)
        if isNull existUser then
            this.ModelState.AddModelError("Email", "このメールアドレスは登録されていません")
            let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"] else "Light"
            this.ViewData.["Theme"] <- cookie
            this.View("Login") :> ActionResult
        else
            if existUser.Password <> trimmedPassword then
                this.ModelState.AddModelError("Password", "パスワードが違います")
                let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"] else "Light"
                this.ViewData.["Theme"] <- cookie
                this.View("Login") :> ActionResult
            else
               let claims = [Claim(ClaimTypes.NameIdentifier, existUser.Id.ToString())]
               let claimsIdentity = ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)
               let authProperties = AuthenticationProperties()
               authProperties.IsPersistent <- true
               authProperties.ExpiresUtc <- DateTimeOffset.UtcNow.AddMinutes(30.0)
               this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ClaimsPrincipal(claimsIdentity), authProperties) |> ignore
               this.RedirectToAction("Index") :> ActionResult
    member this.Login()=
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