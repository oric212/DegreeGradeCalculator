using System.Net.WebSockets;
using System.Text.Json;
using DegreeGradeCalculator;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using DegreeGradeCalculator.TextImport;

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
    builder.Services.AddSingleton<ITextCourseParser, TextCourseParser>();
    builder.WebHost.ConfigureKestrel(o => { o.Listen(IPAddress.Loopback, 0); o.Limits.MaxRequestBodySize = 5 * 1024 * 1024; });
    var app = builder.Build(); var store = new Store(directory);
    // Windowed launches default to local mode; explicit configuration overrides either launch mode.
    var localMode = builder.Configuration.GetValue<bool?>("LocalAppMode") ?? !args.Contains("--no-browser");
    var sessions = new LocalSessions(TimeProvider.System, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(60));
    app.MapGet("/api/session/config", () => Results.Ok(new { localAppMode = localMode, heartbeatMilliseconds = 3000 }));
    app.MapPost("/api/session/heartbeat", (BrowserSession session) => !localMode ? Results.NoContent() :
        sessions.Heartbeat(session.Id) ? Results.NoContent() : Results.BadRequest());
    app.MapPost("/api/session/close", (BrowserSession session) => { if (localMode) sessions.Close(session.Id); return Results.NoContent(); });
    IResult Conflict(RevisionConflictException e) => Results.Conflict(new { error = "revision_conflict", currentRevision = e.CurrentRevision });

    app.Use(async (context, next) =>
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (context.Request.Host.Host != "127.0.0.1" || (origin.Length > 0 && origin != $"http://{context.Request.Host}")) { context.Response.StatusCode = 403; return; }
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self'; connect-src 'self'; object-src 'none'; frame-ancestors 'none'";
        // Release assets must refresh when the executable is reopened after an update.
        if (!context.Request.Path.StartsWithSegments("/api")) context.Response.Headers.CacheControl = "no-store";
        await next();
    });
    app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(5), KeepAliveTimeout = TimeSpan.FromSeconds(10) });
    app.Map("/api/session/socket", async (HttpContext context) =>
    {
        if (!localMode || !context.WebSockets.IsWebSocketRequest ||
            !Guid.TryParse(context.Request.Query["id"], out var id) || !sessions.Heartbeat(id))
        { context.Response.StatusCode = 400; return; }
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, app.Lifetime.ApplicationStopping);
        // Native socket ping/pong keeps background tabs alive even if browser timers are throttled.
        var refresh = Task.Run(async () =>
        {
            try {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
                while (await timer.WaitForNextTickAsync(cancellation.Token)) sessions.Heartbeat(id);
            } catch (OperationCanceledException) { }
        });
        try {
            var buffer = new byte[128];
            while (!cancellation.IsCancellationRequested) {
                var received = await socket.ReceiveAsync(buffer, cancellation.Token);
                if (received.MessageType == WebSocketMessageType.Close) {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); break;
                }
            }
        } catch (Exception e) when (e is WebSocketException or OperationCanceledException) { }
        finally { cancellation.Cancel(); await refresh; sessions.Close(id); }
    });
    app.UseDefaultFiles(); app.UseStaticFiles();
    app.MapGet("/api/data", () => store.Read());
    app.MapPost("/api/import/parse-text", (ParseTextRequest request, ITextCourseParser parser) => {
        try { return Results.Ok(parser.Parse(request)); }
        catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
    });
    app.MapPost("/api/import/preview", (CourseImportRequest request) => {
        try { return Results.Ok(CourseTextImport.Preview(store.Read(), request)); }
        catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
    });
    app.MapPost("/api/import/courses", (CourseImportRequest request) => {
        try { if (!request.Confirmed) return Results.BadRequest(new { error = "confirmation-required" }); return Results.Ok(store.Update(request.Revision ?? -1, b => CourseTextImport.Apply(b, request) with { Revision = checked(b.Revision + 1) })); }
        catch (RevisionConflictException e) { return Conflict(e); }
        catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
    });
    app.MapPut("/api/data", (JsonElement document) => { try {
        if (document.ValueKind != JsonValueKind.Object) return Results.BadRequest(new { error = "Invalid data." });
        if (!document.TryGetProperty("revision", out var supplied) || supplied.ValueKind != JsonValueKind.Number || !supplied.TryGetInt64(out _))
            return Conflict(new RevisionConflictException(store.Read().Revision));
        var b = document.Deserialize<Backup>(Store.Json) ?? throw new ArgumentException("Invalid data.");
        var revision = store.Write(b); return Results.Ok(new { revision }); } catch (RevisionConflictException e) { return Conflict(e); } catch (JsonException e) { return Results.BadRequest(new { error = e.Message }); } catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); } });
    app.MapPost("/api/validate", (Backup b) => { try { Validation.Check(b); return Results.Ok(); } catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); } });
    app.MapPost("/api/shutdown", () => { app.Lifetime.StopApplication(); return Results.Ok(); });
    await app.StartAsync();
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    File.WriteAllText(Path.Combine(directory, "address.txt"), address);
    if (!args.Contains("--no-browser")) { try { Process.Start(new ProcessStartInfo(address) { UseShellExecute = true }); } catch (Exception e) { File.WriteAllText(Path.Combine(directory, "launch-error.txt"), e.Message); } }
    using var monitorCancellation = new CancellationTokenSource();
    var monitor = localMode ? Task.Run(async () =>
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(monitorCancellation.Token))
                if (sessions.ShouldStop()) { app.Lifetime.StopApplication(); break; }
        }
        catch (OperationCanceledException) { }
    }) : Task.CompletedTask;
    await app.WaitForShutdownAsync();
    monitorCancellation.Cancel();
    await monitor;
}
