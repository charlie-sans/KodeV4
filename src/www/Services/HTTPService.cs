using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KodeRunner.Services
{
    internal class HTTPService : IService
    {
        private readonly Dictionary<string, Dictionary<string, Func<HttpContext, Task>>> _httpRoutes = new();
        private readonly Dictionary<string, Func<WebSocket, Task>> _wsRoutes = new();

        public HTTPService()
        {
            // Scan for routes
            ScanForRoutes();
        }
        /// <summary>
        /// Gets the collection of HTTP routes mapped to their corresponding request handlers.
        /// </summary>
        /// <returns>A dictionary where each key is an HTTP method (such as "GET" or "POST"), and each value is another
        /// dictionary mapping route paths to handler functions. Each handler function processes an HTTP request and
        /// returns a task representing the asynchronous operation.</returns>
        public Dictionary<string, Dictionary<string, Func<HttpContext, Task>>> GetHttpRoutes()
        {
            return _httpRoutes;
        }
        /// <summary>
        /// Gets the collection of WebSocket route handlers registered with the server.
        /// </summary>
        /// <remarks>The returned dictionary provides direct access to the registered WebSocket routes and
        /// their handlers. Modifying the dictionary may affect the server's routing behavior.</remarks>
        /// <returns>A dictionary mapping route paths to handler functions. Each handler is a function that processes a WebSocket
        /// connection for the associated route.</returns>
        public Dictionary<string, Func<WebSocket, Task>> GetWebSocketRoutes()
        {
            return _wsRoutes;
        }
        /// <summary>
        /// Writes a list of all registered HTTP and WebSocket routes to the console output.
        /// </summary>
        /// <remarks>Use this method for diagnostic or debugging purposes to view the currently registered
        /// routes managed by the service. The output includes both HTTP and WebSocket routes, grouped by
        /// type.</remarks>
        public void DumpRoutes()
        {
            Console.WriteLine("these routes were found by the HTTPService");
            Console.WriteLine("Registered Routes:");
            Console.WriteLine("HTTP Routes:");
            foreach (var method in _httpRoutes.Keys)
            {
                foreach (var path in _httpRoutes[method].Keys)
                {
                    Console.WriteLine($"  [{method}] {path}");
                }
            }
            Console.WriteLine("WebSocket Routes:");
            foreach (var path in _wsRoutes.Keys)
            {
                Console.WriteLine($"  [WS] {path}");
            }
        }

        public void ScanFolderForRoutes(string folderpath)
        {
        }
        private void ScanForRoutes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<ServiceAttribute>() != null)
                    {
                        var instance = Activator.CreateInstance(type); // assuming default ctor
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var routeAttr = method.GetCustomAttribute<RouteAttribute>();
                            if (routeAttr != null)
                            {
                                var key = routeAttr.Method.ToUpper();
                                if (!_httpRoutes.ContainsKey(key))
                                    _httpRoutes[key] = new();
                                _httpRoutes[key][routeAttr.Path] = (HttpContext ctx) => (Task)method.Invoke(instance, new object[] { ctx });
                            }

                            var wsAttr = method.GetCustomAttribute<WebSocketRouteAttribute>();
                            if (wsAttr != null)
                            {
                                _wsRoutes[wsAttr.Path] = (WebSocket ws) => (Task)method.Invoke(instance, new object[] { ws });
                            }

                            var razorAttr = method.GetCustomAttribute<RazorRouteAttribute>();
                            if (razorAttr != null)
                            {
                                var key = razorAttr.Method.ToUpper();
                                if (!_httpRoutes.ContainsKey(key))
                                    _httpRoutes[key] = new();

                                _httpRoutes[key][razorAttr.Path] = async (HttpContext ctx) =>
                                {
                                    try
                                    {
                                    // Allow razor handler methods to optionally return a model object or Task<object>.
                                    object model = null;
                                    var parameters = method.GetParameters();
                                    object[] args = parameters.Length == 1 && parameters[0].ParameterType == typeof(HttpContext)
                                        ? new object[] { ctx }
                                        : Array.Empty<object>();

                                    var ret = method.Invoke(instance, args);
                                    if (ret is Task task)
                                    {
                                        await task;
                                        var prop = task.GetType().GetProperty("Result");
                                        model = prop?.GetValue(task);
                                    }
                                    else
                                    {
                                        model = ret;
                                    }

                                        await RenderViewAsync(ctx, razorAttr.ViewName, model);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"RazorRoute handler threw: {ex}");
                                        ctx.Response.StatusCode = 500;
                                        await ctx.Response.WriteAsync("Internal server error");
                                    }
                                };
                                Console.WriteLine($"Registered RazorRoute: [{key}] {razorAttr.Path} -> {razorAttr.ViewName}");
                            }
                        }
                    }
                }
            }
        }

        public async Task StartServer(int port = 8080, CancellationToken token = default)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.ConfigureServices(services =>
                    {
                        // Ensure Razor Pages are only picked up from the /Pages root directory.
                        // This prevents razor pages being scattered across other folders (eg. Views).
                        services.AddRazorPages(options =>
                        {
                            // Use the logical Pages root at runtime. The publish/output step will ensure
                            // a `Pages` folder exists in the app content root (we copy source files there).
                            options.RootDirectory = "/Pages";
                        });

                        // Keep controllers and views if the app uses controllers, but restrict
                        // the view engine to look inside /Pages only so organized pages aren't
                        // silently served from other folders.
                        services.AddControllersWithViews();

                        // Restrict view lookup to the /Pages folder to avoid scatter.
                        services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(opts =>
                        {
                            // Clear default locations (which include /Views) and set to the Pages paths
                            // used at runtime (a `Pages` folder under the app content root).
                            opts.ViewLocationFormats.Clear();
                            opts.ViewLocationFormats.Add("/src/www/Pages/{1}/{0}.cshtml");
                            opts.ViewLocationFormats.Add("/src/www/Pages/Shared/{0}.cshtml");

                            // Area locations also adjusted so Areas (if used) still map under /Pages
                            opts.AreaViewLocationFormats.Clear();
                            opts.AreaViewLocationFormats.Add("/Areas/{2}/Pages/{1}/{0}.cshtml");
                            opts.AreaViewLocationFormats.Add("/Areas/{2}/Pages/Shared/{0}.cshtml");
                        });
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseWebSockets();
                        app.UseRouting();
                        app.Use(async (context, next) =>
                        {
                            if (context.WebSockets.IsWebSocketRequest)
                            {
                                var path = context.Request.Path.Value;
                                if (_wsRoutes.TryGetValue(path, out var handler))
                                {
                                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                                    await handler(ws);
                                }
                                else
                                {
                                    context.Response.StatusCode = 404;
                                }
                            }
                            else
                            {
                                var method = context.Request.Method.ToUpper();
                                var path = context.Request.Path.Value;
                                Console.WriteLine($"Incoming request: [{method}] {path}");
                                if (_httpRoutes.TryGetValue(method, out var routes) && routes.TryGetValue(path, out var handler))
                                {
                                    Console.WriteLine($"Matched route: [{method}] {path}");
                                    await handler(context);
                                }
                                else
                                {
                                    Console.WriteLine($"No match for {method} {path}");
                                    await next();
                                }
                            }
                        });
                        
                        // Add helper extension available to handle direct view render endpoints
                        app.Use(async (ctx, nextMiddleware) =>
                        {
                            // If previous middleware handled endpoint, skip; otherwise continue
                            await nextMiddleware();
                        });
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapRazorPages();
                            endpoints.MapControllers();
                        });
                    });
                    webBuilder.UseUrls($"http://localhost:{port}");
                })
                .Build();

            await host.RunAsync(token);
        }

        /// <summary>
        /// Render a Razor view (or page) and write HTML to the response.
        /// View resolution uses the configured RazorViewEngine locations.
        /// </summary>
        private static async Task RenderViewAsync(HttpContext ctx, string viewName, object model = null)
        {
            var viewEngine = ctx.RequestServices.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = ctx.RequestServices.GetRequiredService<ITempDataProvider>();

            var actionContext = new ActionContext(ctx, ctx.GetRouteData(), new ActionDescriptor());

            // Try FindView first, then GetView (explicit path)
            var viewResult = viewEngine.FindView(actionContext, viewName, isMainPage: false);
            if (!viewResult.Success)
            {
                viewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);
            }

            if (!viewResult.Success)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.WriteAsync($"View '{viewName}' not found");
                return;
            }

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            using var sw = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                new TempDataDictionary(ctx, tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.WriteAsync(sw.ToString());
        }
    }
}
