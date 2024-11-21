namespace johma_site.Extensions

 open System.Runtime.CompilerServices
 open System.Text.RegularExpressions


 [<Extension>]
    type StringExtensions() =
              [<Extension>]
               static member StripHtmlTags (input: string) =
                 Regex.Replace(input, "<.*?>", "")     