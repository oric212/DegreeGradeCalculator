using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DegreeGradeCalculator;
using DegreeGradeCalculator.TextImport;
using Xunit;

public class ParserEndpointTests
{
    [Fact]
    public async Task RealEndpointIsReadOnlyAndImportRequiresConfirmation()
    {
        var directory = Path.Combine(Path.GetTempPath(), "parser-http-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        var backup = new Backup { Degrees = [new Degree { Name = "Demo", Years = [new AcademicYear { Semesters = [new Semester()] }] }] };
        new Store(directory).Write(backup);
        using var process = new Process { StartInfo = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true } };
        process.StartInfo.ArgumentList.Add(typeof(Store).Assembly.Location);
        process.StartInfo.ArgumentList.Add("--no-browser");
        process.StartInfo.Environment["GRADEPILOT_DATA"] = directory;
        using var http = new HttpClient();
        try
        {
            process.Start();
            var addressFile = Path.Combine(directory, "address.txt");
            for (var i = 0; i < 100 && !File.Exists(addressFile) && !process.HasExited; i++) await Task.Delay(100);
            Assert.True(File.Exists(addressFile), "Application failed to start");
            http.BaseAddress = new Uri(await File.ReadAllTextAsync(addressFile));
            foreach (var asset in new[] { "/", "/app.js", "/style.css", "/text-import.js" })
            {
                var staticResponse = await http.GetAsync(asset);
                staticResponse.EnsureSuccessStatusCode();
                Assert.True(staticResponse.Headers.CacheControl?.NoStore);
            }
            var before = await http.GetStringAsync("/api/data");
            foreach (var text in new[] { "Algorithms|4|82", "Math,2.5,45", "OS\t5\t68", "שם קורס|נק״ז|ציון\nאלגוריתמים|4|82", "Name|נקז|Grade\nDatabases|3.5|91" })
            {
                var response = await http.PostAsJsonAsync("/api/import/parse-text", new ParseTextRequest(text));
                response.EnsureSuccessStatusCode();
                var parsed = await response.Content.ReadFromJsonAsync<ParseTextResult>();
                Assert.True(parsed!.Success); Assert.Single(parsed.Courses);
            }
            Assert.Equal(before, await http.GetStringAsync("/api/data"));
            var oversized = await http.PostAsJsonAsync("/api/import/parse-text", new ParseTextRequest(new string('x', 50_001)));
            Assert.Equal(HttpStatusCode.BadRequest, oversized.StatusCode);
            var request = new CourseImportRequest(backup.Degrees[0].Id, 1, "A", [new ReviewedCourse { Name = "Math", Credits = 2.5m, Grade = 45 }]);
            var preview = await http.PostAsJsonAsync("/api/import/preview", request); preview.EnsureSuccessStatusCode();
            Assert.Equal(before, await http.GetStringAsync("/api/data"));
            var unconfirmed = await http.PostAsJsonAsync("/api/import/courses", request);
            Assert.Equal(HttpStatusCode.BadRequest, unconfirmed.StatusCode);
            Assert.Equal(before, await http.GetStringAsync("/api/data"));
            var imported = await http.PostAsJsonAsync("/api/import/courses", request with { Confirmed = true }); imported.EnsureSuccessStatusCode();
            var saved = JsonSerializer.Deserialize<Backup>(await http.GetStringAsync("/api/data"), Store.Json)!;
            Assert.Equal(45, Assert.Single(Calculation.Courses(saved.Degrees[0])).Grade);
            var identical = await http.PostAsJsonAsync("/api/import/preview", request);
            var same = await identical.Content.ReadFromJsonAsync<ImportPreview>();
            Assert.Equal(0, same!.Count); Assert.Equal(1, same.Unchanged);
            var update = request with { Rows = [new ReviewedCourse { Name = "Math", Credits = 3, Grade = 81 }] };
            var reviewed = await http.PostAsJsonAsync("/api/import/preview", update);
            var changes = await reviewed.Content.ReadFromJsonAsync<ImportPreview>();
            Assert.Equal(1, changes!.Updated); Assert.Equal(45, changes.Updates[0].OldGrade);
            var changed = await http.PostAsJsonAsync("/api/import/courses", update with { Confirmed = true, ReviewToken = changes.ReviewToken });
            changed.EnsureSuccessStatusCode();
            saved = JsonSerializer.Deserialize<Backup>(await http.GetStringAsync("/api/data"), Store.Json)!;
            Assert.Equal(81, Assert.Single(Calculation.Courses(saved.Degrees[0])).Grade);
        }
        finally
        {
            if (!process.HasExited) { if (http.BaseAddress is not null) await http.PostAsync("/api/shutdown", null); if (!process.WaitForExit(5000)) process.Kill(true); }
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            Directory.Delete(directory, true);
        }
    }
}
