namespace Microsoft.AspNet.Diagnostics.Entity.Views
{
#line 1 "DatabaseErrorPage.cshtml"
using System

#line default
#line hidden
    ;
#line 2 "DatabaseErrorPage.cshtml"
using System.Linq

#line default
#line hidden
    ;
#line 3 "DatabaseErrorPage.cshtml"
using JetBrains.Annotations;

#line default
#line hidden
#line 4 "DatabaseErrorPage.cshtml"
using Microsoft.AspNet.Diagnostics.Entity

#line default
#line hidden
    ;
#line 5 "DatabaseErrorPage.cshtml"
using Microsoft.AspNet.Diagnostics.Entity.Utilities;

#line default
#line hidden
#line 6 "DatabaseErrorPage.cshtml"
using Microsoft.AspNet.Diagnostics.Entity.Views

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class DatabaseErrorPage : Microsoft.AspNet.Diagnostics.Views.BaseView
    {
#line 14 "DatabaseErrorPage.cshtml"

    private DatabaseErrorPageModel _model;

    public virtual DatabaseErrorPageModel Model
    {
        get { return _model;}
        [param: NotNull] set
        {
            Check.NotNull(value, "value");

            _model = value;
        }
    }

#line default
#line hidden
        #line hidden
        public DatabaseErrorPage()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
#line 7 "DatabaseErrorPage.cshtml"
  
    Response.StatusCode = 500;
    // TODO: Response.ReasonPhrase = "Internal Server Error";
    Response.ContentType = "text/html";
    Response.ContentLength = null; // Clear any prior Content-Length

#line default
#line hidden

            WriteLiteral("\r\n");
            WriteLiteral("<!DOCTYPE html>\r\n\r\n<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\r\n    <he" +
"ad>\r\n        <meta charset=\"utf-8\" />\r\n        <title>Internal Server Error</title>\r\n        <style>\r\n            body {\r\n    font-family: 'Segoe UI', Tahoma, Arial, Helvetica, sans-serif;\r\n    font-size: .813em;\r\n    line-height: 1.4em;\r\n    color: #222;\r\n}\r\n\r\nh1, h2, h3, h4, h5 {\r\n    font-weight: 100;\r\n}\r\n\r\nh1 {\r\n    color: #44525e;\r\n    margin: 15px 0 15px 0;\r\n}\r\n\r\nh2 {\r\n    margin: 10px 5px 0 0;\r\n}\r\n\r\nh3 {\r\n    color: #363636;\r\n    margin: 5px 5px 0 0;\r\n}\r\n\r\ncode {\r\n    font-family: Consolas, \"Courier New\", courier, monospace;\r\n}\r\n\r\na {\r\n    color: #1ba1e2;\r\n    text-decoration: none;\r\n}\r\n\r\n    a:hover {\r\n        color: #13709e;\r\n        text-decoration: underline;\r\n    }\r\n\r\nhr {\r\n    border: 1px #ddd solid;\r\n}\r\n\r\nbody .titleerror {\r\n    padding: 3px;\r\n}\r\n\r\n#applyMigrations {\r\n    font-size: 14px;\r\n    background: #44c5f2;\r\n    color: #ffffff;\r\n    display: inline-block;\r\n    padding: 6px 12px;\r\n    margin-bottom: 0;\r\n    font-weight: normal;\r\n    text-align: center;\r\n    white-space: nowrap;\r\n    vertical-align: middle;\r\n    cursor: pointer;\r\n    border: 1px solid transparent;\r\n}\r\n\r\n    #applyMigrations:disabled {\r\n        background-color: #a9e4f9;\r\n        border-color: #44c5f2;\r\n    }\r\n\r\n.error {\r\n    color: red;\r\n}\r\n\r\n.expanded {\r\n    display: block;\r\n}\r\n\r\n.collapsed {\r\n    display: none;\r\n}\r\n\r\n            ");
            Write(
#line 36 "DatabaseErrorPage.cshtml"
             string.Empty

#line default
#line hidden
            );

            WriteLiteral("\r\n        </style>\r\n    </head>\r\n    <body>\r\n        <h1>A database operartion fa" +
"iled while processing the request.</h1>\r\n");
#line 41 "DatabaseErrorPage.cshtml"
        

#line default
#line hidden

#line 41 "DatabaseErrorPage.cshtml"
         if (Model.Options.ShowExceptionDetails)
        {

#line default
#line hidden

            WriteLiteral("            <p>\r\n");
#line 44 "DatabaseErrorPage.cshtml"
                

#line default
#line hidden

#line 44 "DatabaseErrorPage.cshtml"
                 for (Exception ex = Model.Exception; ex != null; ex = ex.InnerException)
                {

#line default
#line hidden

            WriteLiteral("                    <span>");
            Write(
#line 46 "DatabaseErrorPage.cshtml"
                           ex.GetType().Name

#line default
#line hidden
            );

            WriteLiteral(": ");
            Write(
#line 46 "DatabaseErrorPage.cshtml"
                                               ex.Message

#line default
#line hidden
            );

            WriteLiteral("</span>\r\n                    <br />\r\n");
#line 48 "DatabaseErrorPage.cshtml"
                }

#line default
#line hidden

            WriteLiteral("            </p>\r\n            <hr />\r\n");
#line 51 "DatabaseErrorPage.cshtml"
        }

#line default
#line hidden

            WriteLiteral("\r\n\r\n");
#line 54 "DatabaseErrorPage.cshtml"
        

#line default
#line hidden

#line 54 "DatabaseErrorPage.cshtml"
         if (Model.Options.ShowMigrationStatus)
        {
            

#line default
#line hidden

#line 56 "DatabaseErrorPage.cshtml"
             if (!Model.DatabaseExists
                 && !Model.PendingMigrations.Any())
             {

#line default
#line hidden

            WriteLiteral("                 <h2>");
            Write(
#line 59 "DatabaseErrorPage.cshtml"
                      Strings.FormatDatabaseErrorPage_NoDbOrMigrationsTitle(Model.ContextType.Name)

#line default
#line hidden
            );

            WriteLiteral("</h2>\r\n                 <p>");
            Write(
#line 60 "DatabaseErrorPage.cshtml"
                     Strings.DatabaseErrorPage_NoDbOrMigrationsInfo

#line default
#line hidden
            );

            WriteLiteral("</p>\r\n                 <code> ");
            Write(
#line 61 "DatabaseErrorPage.cshtml"
                         Strings.DatabaseErrorPage_AddMigrationCommand

#line default
#line hidden
            );

            WriteLiteral(" </code>\r\n                 <br />\r\n                 <code> ");
            Write(
#line 63 "DatabaseErrorPage.cshtml"
                         Strings.DatabaseErrorPage_UpdateDatabaseCommand

#line default
#line hidden
            );

            WriteLiteral(" </code>\r\n                 <hr />\r\n");
#line 65 "DatabaseErrorPage.cshtml"
             }
             else
             {
                 if (Model.PendingMigrations.Any())
                 {

#line default
#line hidden

            WriteLiteral("                     <div>\r\n                         <h2>");
            Write(
#line 71 "DatabaseErrorPage.cshtml"
                              Strings.FormatDatabaseErrorPage_PendingMigrationsTitle(Model.ContextType.Name)

#line default
#line hidden
            );

            WriteLiteral("</h2>\r\n                         <p>");
            Write(
#line 72 "DatabaseErrorPage.cshtml"
                             Strings.FormatDatabaseErrorPage_PendingMigrationsInfo(Model.ContextType.Name)

#line default
#line hidden
            );

            WriteLiteral("</p>\r\n                         <ul>\r\n");
#line 74 "DatabaseErrorPage.cshtml"
                             

#line default
#line hidden

#line 74 "DatabaseErrorPage.cshtml"
                              foreach (var migration in Model.PendingMigrations)
                             {

#line default
#line hidden

            WriteLiteral("                                 <li>");
            Write(
#line 76 "DatabaseErrorPage.cshtml"
                                      migration

#line default
#line hidden
            );

            WriteLiteral("</li>\r\n");
#line 77 "DatabaseErrorPage.cshtml"
                             }

#line default
#line hidden

            WriteLiteral("\r\n                         </ul>\r\n");
#line 79 "DatabaseErrorPage.cshtml"
                         

#line default
#line hidden

#line 79 "DatabaseErrorPage.cshtml"
                          if (Model.Options.EnableMigrationCommands)
                         {

#line default
#line hidden

            WriteLiteral("                             <p>\r\n                                 <button id=\"ap" +
"plyMigrations\" onclick=\" ApplyMigrations() \">");
            Write(
#line 82 "DatabaseErrorPage.cshtml"
                                                                                             Strings.DatabaseErrorPage_ApplyMigrationsButton

#line default
#line hidden
            );

            WriteLiteral(@"</button>
                                 <span id=""applyMigrationsError"" class=""error""></span>
                                 <span id=""applyMigrationsSuccess""></span>
                             </p>
                             <script>
                                 function ApplyMigrations() {
                                     applyMigrations.disabled = true;
                                     applyMigrationsError.innerHTML = """";
                                     applyMigrations.innerHTML = """);
            Write(
#line 90 "DatabaseErrorPage.cshtml"
                                                                   Strings.DatabaseErrorPage_ApplyMigrationsButtonRunning

#line default
#line hidden
            );

            WriteLiteral("\";\r\n\r\n                                     var req = new XMLHttpRequest();\r\n     " +
"                                req.open(\"POST\", \"");
            Write(
#line 93 "DatabaseErrorPage.cshtml"
                                                        Model.Options.MigrationsEndPointPath.Value

#line default
#line hidden
            );

            WriteLiteral("\", true);\r\n                                     var params = \"context=\" + encodeU" +
"RIComponent(\"");
            Write(
#line 94 "DatabaseErrorPage.cshtml"
                                                                                    Model.ContextType.AssemblyQualifiedName

#line default
#line hidden
            );

            WriteLiteral(@""");
                                     req.setRequestHeader(""Content-type"", ""application/x-www-form-urlencoded"");
                                     req.setRequestHeader(""Content-length"", params.length);
                                     req.setRequestHeader(""Connection"", ""close"");

                                     req.onload = function(e) {
                                         if (req.status == 204) {
                                             applyMigrations.innerHTML = """);
            Write(
#line 101 "DatabaseErrorPage.cshtml"
                                                                           Strings.DatabaseErrorPage_ApplyMigrationsButtonDone

#line default
#line hidden
            );

            WriteLiteral("\";\r\n                                             applyMigrationsSuccess.innerHTML" +
" = \"<a href=\'.\'>");
            Write(
#line 102 "DatabaseErrorPage.cshtml"
                                                                                              Strings.DatabaseErrorPage_MigrationsAppliedRefresh

#line default
#line hidden
            );

            WriteLiteral(@"</a>"";
                                         } else {
                                             ErrorApplyingMigrations();
                                         }
                                     };

                                     req.onerror = function(e) {
                                         ErrorApplyingMigrations();
                                     };

                                     req.send(params);
                                 }

                                 function ErrorApplyingMigrations() {
                                     applyMigrations.innerHTML = """);
            Write(
#line 116 "DatabaseErrorPage.cshtml"
                                                                   Strings.DatabaseErrorPage_ApplyMigrationsButton

#line default
#line hidden
            );

            WriteLiteral("\";\r\n                                     applyMigrationsError.innerHTML = \"");
            Write(
#line 117 "DatabaseErrorPage.cshtml"
                                                                        Strings.DatabaseErrorPage_ApplyMigrationsFailed

#line default
#line hidden
            );

            WriteLiteral("\";\r\n                                     applyMigrations.disabled = false;\r\n     " +
"                            }\r\n                             </script>\r\n");
#line 121 "DatabaseErrorPage.cshtml"
                         }

#line default
#line hidden

            WriteLiteral("\r\n                         <p>");
            Write(
#line 122 "DatabaseErrorPage.cshtml"
                             Strings.DatabaseErrorPage_HowToApplyFromCmd

#line default
#line hidden
            );

            WriteLiteral("</p>\r\n                         <code>");
            Write(
#line 123 "DatabaseErrorPage.cshtml"
                                Strings.DatabaseErrorPage_UpdateDatabaseCommand

#line default
#line hidden
            );

            WriteLiteral("</code>\r\n                         <hr />\r\n                     </div>\r\n");
#line 126 "DatabaseErrorPage.cshtml"
                 }
                 else if (Model.PendingModelChanges)
                 {

#line default
#line hidden

            WriteLiteral("                     <div>\r\n                         <h2>");
            Write(
#line 130 "DatabaseErrorPage.cshtml"
                              Strings.FormatDatabaseErrorPage_PendingChangesTitle(Model.ContextType.Name)

#line default
#line hidden
            );

            WriteLiteral("</h2>\r\n                         <p>");
            Write(
#line 131 "DatabaseErrorPage.cshtml"
                             Strings.DatabaseErrorPage_PendingChangesInfo

#line default
#line hidden
            );

            WriteLiteral("</p>\r\n                         <code>");
            Write(
#line 132 "DatabaseErrorPage.cshtml"
                                Strings.DatabaseErrorPage_AddMigrationCommand

#line default
#line hidden
            );

            WriteLiteral("</code>\r\n                         <br />\r\n                         <code>");
            Write(
#line 134 "DatabaseErrorPage.cshtml"
                                Strings.DatabaseErrorPage_UpdateDatabaseCommand

#line default
#line hidden
            );

            WriteLiteral("</code>\r\n                         <hr />\r\n                     </div>\r\n");
#line 137 "DatabaseErrorPage.cshtml"
                 }
             }

#line default
#line hidden

#line 138 "DatabaseErrorPage.cshtml"
              
        }

#line default
#line hidden

            WriteLiteral("    </body>\r\n</html>");
        }
        #pragma warning restore 1998
    }
}
