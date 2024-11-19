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
    
    
    let ThemeApply(controller: HomeController) =
         let cookie = if controller.Request.Cookies.ContainsKey("Theme") then controller.Request.Cookies.["Theme"]  else "Light"
         controller.ViewData.["Theme"] <- cookie
        
    member this.Index () =
        let cookie = if this.Request.Cookies.ContainsKey("Theme") then this.Request.Cookies.["Theme"] 
        
                        else
                                let newCookie = "Light"
                                let cookieOptions = CookieOptions()
                                cookieOptions.Expires <- DateTimeOffset.UtcNow.AddDays(30.0)
                                this.Response.Cookies.Append("Theme", newCookie, cookieOptions)
                                newCookie
        this.ViewData.["Theme"] <- cookie                        
        let users = db.Users.ToList()
        if users = null then
           this.View(new List<User>())
         else
        this.View(users)                      
         
    member this.Profile () =
        ThemeApply(this)
        this.View()
        
    member this.Register () =
        ThemeApply(this)
        this.View()
    
    member this.AdminControl() =
       ThemeApply(this)
       let users = db.Users.ToList()
       let safeUsers = 
            if isNull users then 
                new List<User>() 
            else 
                users
       this.View(safeUsers) :> ActionResult
        
        
     member this.UpdateData(userId:int,comboBox: string,updateData:string) : ActionResult =
         let user = db.Users.FirstOrDefault(fun x -> x.Id = userId)
         let mutable hasError = false
         if isNull user then
          this.ModelState.AddModelError("UserId", "ユーザーが見つかりません")
          ThemeApply(this)
          let user = db.Users.ToList()
          this.View("AdminControl",user) :> ActionResult
        else
        match comboBox with
        | "Name" -> user.Name <- updateData
        | "Email" -> user.Email <- updateData
        | "Role" ->
            if String.IsNullOrWhiteSpace(updateData) then
                  this.ModelState.AddModelError("Role", "ロールを入力してください")
                  hasError <- true
            else
                  user.Role <- updateData
        | _ -> ()
        if hasError then
            ThemeApply(this)
            let user = db.Users.ToList()
            this.View("AdminControl",user) :> ActionResult
        else
            db.SaveChanges() |> ignore
            this.RedirectToAction("AdminControl") :> ActionResult
        
    member this.UsrProfile() =
        ThemeApply(this)
        
        let userIdCalaim = this.User.FindFirst(ClaimTypes.NameIdentifier)
        if isNull userIdCalaim then
            this.RedirectToAction("Login") :> ActionResult
        else
            let userId = userIdCalaim.Value
            let user = db.Users.FirstOrDefault(fun x -> x.Id = int userId)
            
            if isNull user then
                this.RedirectToAction("Login") :> ActionResult
            else
                let users = [user] |> List.toSeq
                this.View(users) :> ActionResult
                
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
          ThemeApply(this)
          this.View("Register") :> ActionResult
      else
        let newUser = User(Name = name, Email = email, Password = trimmedPassword,Role = "User")
        db.Users.Add(newUser) |> ignore
        db.SaveChanges() |> ignore
        this.RedirectToAction("Login") :> ActionResult
        
        
    member this.LoginAcc(email: String,password: String) : ActionResult =
        let trimmedPassword = password.Trim()
        let existUser = db.Users.FirstOrDefault(fun x -> x.Email = email)
        if isNull existUser then
            this.ModelState.AddModelError("Email", "このメールアドレスは登録されていません")
            ThemeApply(this)
            this.View("Login") :> ActionResult
        else
            if existUser.Password <> trimmedPassword then
                this.ModelState.AddModelError("Password", "パスワードが違います")
                ThemeApply(this)
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
        ThemeApply(this)
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