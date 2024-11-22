namespace johma_site.Controllers

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Runtime.CompilerServices
open System.Security.Claims
open System.Text.RegularExpressions
open System.Threading.Tasks
open System.Diagnostics

open Ganss.Xss
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open johma_site.Models
open johma_site
open DataLibrary
open Microsoft.EntityFrameworkCore

type blogUserViewModel() =
    member val Blog = Blog() with get, set
    member val Users = List<User>() with get, set
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
       
       
    member this.Blog() =
        ThemeApply(this)
        let blogs = db.Blogs.ToList()
        let userRole = 
            if not (isNull this.User) then
                this.User.Claims
                    .FirstOrDefault(fun c -> c.Type = "Role")
                    |> Option.ofObj
                    |> Option.map (fun c -> c.Value)
                    |> Option.defaultValue ""
            else ""
        this.ViewData.["UserRole"] <- userRole
        this.View(blogs)
        
    member this.CreateBlog(id: int,edit: bool) : ActionResult =
        let blogID = id
        let blogModel =
            if edit then
                db.Blogs.FirstOrDefault(fun x -> x.Id = blogID)
            else
                Blog()
        ThemeApply(this)
        let users = db.Users.ToList()
        let model = blogUserViewModel(Blog = blogModel, Users = users)
        this.ViewData.["IsEdit"] <- edit
        this.View(model)
        
    member this.DeleteBlog(id: int) : ActionResult =
        let blog = db.Blogs.FirstOrDefault(fun x -> x.Id = id)
        if isNull blog then
                this.RedirectToAction("Blog") :> ActionResult
        else
            db.Blogs.Remove(blog) |> ignore
            db.SaveChanges() |> ignore
            this.RedirectToAction("Blog") :> ActionResult
            
    member this.EditBlog(id: int, title: string, content: string, author: string, imagePath: string, date: DateTime) : ActionResult =
        let existingBlog = db.Blogs.FirstOrDefault(fun x -> x.Id = id)
        if isNull existingBlog then
            this.RedirectToAction("Blog") :> ActionResult
        else
                existingBlog.Title <- title
                existingBlog.Content <- content
                existingBlog.Author <- author
                existingBlog.ImagePath <- imagePath
                existingBlog.Date <- date
                db.SaveChanges() |> ignore
                this.TempData.["BlogID"] <- existingBlog.Id
                this.RedirectToAction("CreateBlog", {|id = existingBlog.Id; edit = true|}) :> ActionResult
            
    member this.EditBlogContent(model: blogUserViewModel,image: IFormFile) : ActionResult =
        let mutable imagePath = ""
        if this.ModelState.IsValid then
                if (String.IsNullOrWhiteSpace(model.Blog.ImagePath)) then
                    imagePath <- Path.Combine("wwwroot/logo-black.png")                
                else
                    imagePath <- Path.Combine("wwwroot/images", image.FileName)
                
                use stream = new FileStream(imagePath, FileMode.Create)   
                if not (String.IsNullOrWhiteSpace(model.Blog.ImagePath)) then
                     image.CopyTo(stream)
                model.Blog.ImagePath <- imagePath
                db.Blogs.Update(model.Blog) |> ignore
                db.SaveChanges() |> ignore
                this.RedirectToAction("Blog") :> ActionResult
        else
           ThemeApply(this)
           this.View(model) :> ActionResult
            
    member this.AddImage(image: IFormFile): ActionResult=
          if this.ModelState.IsValid then
            let imagePath = Path.Combine("wwwroot/images", image.FileName)
            use stream = new FileStream(imagePath, FileMode.Create)
            image.CopyTo(stream)
            this.RedirectToAction("SuccessView") :> ActionResult
          else
               this.ModelState.AddModelError("Image", "画像を選択してください")
               this.View("ErrorView") :> ActionResult
    
     member this.GetBlogImage(imageName: string) : IActionResult =
            let imagePath = Path.Combine("wwwroot/images", imageName)
            if File.Exists(imagePath) then
                let imageStream = new FileStream(imagePath, FileMode.Open)
                this.File(imageStream, "image/jpeg") :> IActionResult
            else
               this.NotFound("ファイルが存在しません") :> IActionResult
        
    member this.BlogDetails(id: int) : ActionResult =
        let blog = db.Blogs.FirstOrDefault(fun x -> x.Id = id)
        if isNull blog then
            this.RedirectToAction("Blog") :> ActionResult
        else
            let sanitizer = HtmlSanitizer()
            blog.Content <- sanitizer.Sanitize(blog.Content)
            ThemeApply(this)
            this.View(blog) :> ActionResult
        
    member this.CreateBlogContent(model: blogUserViewModel,image: IFormFile) : ActionResult =
        let mutable imagePath = ""
        if this.ModelState.IsValid then
                if (String.IsNullOrWhiteSpace(model.Blog.ImagePath)) then
                    imagePath <- Path.Combine("wwwroot/logo-black.png")                
                else
                    imagePath <- Path.Combine("wwwroot/images", image.FileName)
                
                use stream = new FileStream(imagePath, FileMode.Create)   
                if not (String.IsNullOrWhiteSpace(model.Blog.ImagePath)) then
                     image.CopyTo(stream)
                model.Blog.ImagePath <- imagePath
                db.Blogs.Update(model.Blog) |> ignore
                db.SaveChanges() |> ignore
                this.RedirectToAction("Blog") :> ActionResult
        else
           ThemeApply(this)
           this.View(model) :> ActionResult
        
        
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
               let claims = [
                   Claim(ClaimTypes.Name, existUser.Email)
                   Claim(ClaimTypes.NameIdentifier, existUser.Id.ToString())
                   Claim(ClaimTypes.Role, existUser.Role)
                   Claim("Role", existUser.Role) 
                  ]
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
    
                  
    member this.Oshi() =
        ThemeApply(this)
        this.View()
        
    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })