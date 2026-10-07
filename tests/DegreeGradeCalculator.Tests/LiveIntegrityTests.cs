using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using DegreeGradeCalculator;
using Xunit;

public sealed class LiveIntegrityTests
{
    [Fact] public async Task HostedModeRejectsStaleWritesAndMissingTokensAndNeverUsesBrowserShutdown()
    {
        await using var host = await Host.Start(false);
        var a = (await host.Http.GetFromJsonAsync<Backup>("/api/data"))!;
        var b = JsonSerializer.Deserialize<Backup>(JsonSerializer.Serialize(a,Store.Json),Store.Json)!;
        Assert.Equal(1,a.Revision);
        a.Language = "he";
        var accepted = await host.Http.PutAsJsonAsync("/api/data",a); accepted.EnsureSuccessStatusCode();
        var saved = (await host.Http.GetFromJsonAsync<Backup>("/api/data"))!;
        Assert.Equal(2,saved.Revision);
        var stale = await host.Http.PutAsJsonAsync("/api/data",b);
        Assert.Equal(HttpStatusCode.Conflict,stale.StatusCode);
        var conflict = JsonDocument.Parse(await stale.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("revision_conflict",conflict.GetProperty("error").GetString());
        Assert.Equal(2,conflict.GetProperty("currentRevision").GetInt64());
        Assert.False(conflict.TryGetProperty("degrees",out _));
        var missing = await host.Http.PutAsJsonAsync("/api/data",new {version=1,language="en",degrees=Array.Empty<object>()});
        Assert.Equal(HttpStatusCode.Conflict,missing.StatusCode);
        var invalidToken = await host.Http.PutAsJsonAsync("/api/data",new {version=1,revision="bad",language="en",degrees=Array.Empty<object>()});
        Assert.Equal(HttpStatusCode.Conflict,invalidToken.StatusCode);
        var invalidShape = await host.Http.PutAsJsonAsync("/api/data",new[] {1,2,3});
        Assert.Equal(HttpStatusCode.BadRequest,invalidShape.StatusCode);
        Assert.Equal("he",(await host.Http.GetFromJsonAsync<Backup>("/api/data"))!.Language);
        saved.Language = "en";
        (await host.Http.PutAsJsonAsync("/api/data",saved)).EnsureSuccessStatusCode();
        Assert.Equal(3,(await host.Http.GetFromJsonAsync<Backup>("/api/data"))!.Revision);
        await host.Heartbeat(Guid.NewGuid());
        await Task.Delay(27_000);
        Assert.False(host.Process.HasExited);
        Assert.Equal(3,(await host.Http.GetFromJsonAsync<Backup>("/api/data"))!.Revision);
        var config = await host.Http.GetFromJsonAsync<JsonElement>("/api/session/config");
        Assert.False(config.GetProperty("localAppMode").GetBoolean());
    }
    [Fact] public async Task LocalMultipleSessionsRefreshAndFinalExpirationExitGracefully()
    {
        await using var host = await Host.Start(true);
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        await host.Heartbeat(a); await host.Heartbeat(b);
        await host.Close(a);
        await Task.Delay(7_000);
        await host.Heartbeat(b);
        Assert.False(host.Process.HasExited);
        await host.Close(b);
        await Task.Delay(3_000); // ordinary refresh/reconnect during grace
        var refresh = Guid.NewGuid(); await host.Heartbeat(refresh);
        await Task.Delay(7_000); await host.Heartbeat(refresh);
        Assert.False(host.Process.HasExited);
        var revision = (await host.Http.GetFromJsonAsync<Backup>("/api/data"))!.Revision;
        Assert.Equal(1,revision); // sessions do not advance data revision
        // No unload notification: heartbeat expiration is the fallback.
        await host.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        Assert.Equal(0,host.Process.ExitCode);
    }
    [Fact] public async Task BackgroundSocketSessionSurvivesWithoutJavascriptTimersAndCloseStopsServer()
    {
        await using var host = await Host.Start(true);
        using var socket = new ClientWebSocket();
        var id = Guid.NewGuid();
        await socket.ConnectAsync(new Uri(host.Http.BaseAddress!.ToString().Replace("http:","ws:") + "api/session/socket?id=" + id),CancellationToken.None);
        // Receive pumps protocol ping/pong, as browser networking does in background.
        using var cancel = new CancellationTokenSource();
        var receive = socket.ReceiveAsync(new byte[128],cancel.Token);
        await Task.Delay(27_000);
        Assert.False(host.Process.HasExited);
        (await host.Http.GetAsync("/api/data")).EnsureSuccessStatusCode();
        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,"",CancellationToken.None);
        await receive;
        await host.Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(0,host.Process.ExitCode);
    }
    private sealed class Host : IAsyncDisposable
    {
        public Process Process {get;} = new() {StartInfo = new ProcessStartInfo("dotnet") {UseShellExecute=false,CreateNoWindow=true}};
        public HttpClient Http {get;} = new();
        private readonly string directory = Path.Combine(Path.GetTempPath(),"live-integrity-"+Guid.NewGuid());
        public static async Task<Host> Start(bool local) {
            var host = new Host(); Directory.CreateDirectory(host.directory);
            host.Process.StartInfo.ArgumentList.Add(typeof(Store).Assembly.Location);
            host.Process.StartInfo.ArgumentList.Add("--no-browser");
            host.Process.StartInfo.Environment["GRADEPILOT_DATA"] = host.directory;
            host.Process.StartInfo.Environment["LocalAppMode"] = local.ToString();
            host.Process.Start();
            var address = Path.Combine(host.directory,"address.txt");
            for(int i=0;i<100&&!File.Exists(address)&&!host.Process.HasExited;i++) await Task.Delay(100);
            Assert.True(File.Exists(address),"server did not start");
            host.Http.BaseAddress = new Uri(await File.ReadAllTextAsync(address));
            return host;
        }
        public async Task Heartbeat(Guid id) => (await Http.PostAsJsonAsync("/api/session/heartbeat",new {id})).EnsureSuccessStatusCode();
        public async Task Close(Guid id) => (await Http.PostAsJsonAsync("/api/session/close",new {id})).EnsureSuccessStatusCode();
        public async ValueTask DisposeAsync() {
            if (!Process.HasExited) {
                try { await Http.PostAsync("/api/shutdown",null); await Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
                catch { if(!Process.HasExited) Process.Kill(true); }
            }
            Http.Dispose(); Process.Dispose();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(directory,true);
        }
    }
}
