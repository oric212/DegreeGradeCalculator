using DegreeGradeCalculator;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var directory = Environment.GetEnvironmentVariable("GRADEPILOT_DATA") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DegreeGradeCalculator");
Directory.CreateDirectory(directory);
FileStream instance;
try { instance = new FileStream(Path.Combine(directory, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
catch (IOException) { var f = Path.Combine(directory, "address.txt"); if (File.Exists(f) && !args.Contains("--no-browser")) Process.Start(new ProcessStartInfo(File.ReadAllText(f)) { UseShellExecute = true }); return; }
using (instance)
{
    var root = Directory.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot")) ? AppContext.BaseDirectory : Directory.GetCurrentDirectory();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = root, WebRootPath = Path.Combine(root, "wwwroot") });
    builder.Logging.ClearProviders();
    builder.WebHost.ConfigureKestrel(o => { o.Listen(IPAddress.Loopback, 0); o.Limits.MaxRequestBodySize = 5 * 1024 * 1024; });
    var app = builder.Build(); var store = new Store(directory);
    app.Use(async (context, next) =>
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (context.Request.Host.Host != "127.0.0.1" || (origin.Length > 0 && origin != $"http://{context.Request.Host}")) { context.Response.StatusCode = 403; return; }
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self'; connect-src 'self'; object-src 'none'; frame-ancestors 'none'";
        await next();
    });
    app.UseDefaultFiles(); app.UseStaticFiles();
    app.MapGet("/api/data", () => store.Read());
    app.MapPut("/api/data", (Backup b) => { try { store.Write(b); return Results.Ok(); } catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); } });
    app.MapPost("/api/validate", (Backup b) => { try { Validation.Check(b); return Results.Ok(); } catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); } });
    app.MapPost("/api/shutdown", () => { app.Lifetime.StopApplication(); return Results.Ok(); });
    await app.StartAsync();
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    File.WriteAllText(Path.Combine(directory, "address.txt"), address);
    if (!args.Contains("--no-browser")) { try { Process.Start(new ProcessStartInfo(address) { UseShellExecute = true }); } catch (Exception e) { File.WriteAllText(Path.Combine(directory, "launch-error.txt"), e.Message); } }
    await app.WaitForShutdownAsync();
}
