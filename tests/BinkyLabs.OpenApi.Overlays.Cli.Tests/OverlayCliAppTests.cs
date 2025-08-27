using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BinkyLabs.OpenApi.Overlays.Cli;
using System.Reflection;
using Xunit;

namespace BinkyLabs.OpenApi.Overlays.Cli.Tests
{
    public class OverlayCliAppTests : IDisposable
    {
        private readonly string tempInputFile;
        private readonly string tempOverlayFile;
        private readonly string tempOutputFile;

        public OverlayCliAppTests()
        {
            tempInputFile = Path.GetTempFileName();
            tempOverlayFile = Path.GetTempFileName();
            tempOutputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".out");
            File.WriteAllText(tempInputFile, "{}"); // Minimal valid OpenAPI JSON
            File.WriteAllText(tempOverlayFile, "{}"); // Minimal valid overlay
        }

        public void Dispose()
        {
            if (File.Exists(tempInputFile)) File.Delete(tempInputFile);
            if (File.Exists(tempOverlayFile)) File.Delete(tempOverlayFile);
            if (File.Exists(tempOutputFile)) File.Delete(tempOutputFile);
        }

        [Fact]
        public async Task RunAsync_WithArguments_ReturnsOK()
        {
            var app = new OverlayCliApp();
            var result = await app.RunAsync([tempInputFile, "--overlay", tempOverlayFile]);
            Assert.Equal(0, result);
        }


        [Fact]
        public async Task RunAsync_MissingArguments_ReturnsError()
        {
            var app = new OverlayCliApp();
            var result = await app.RunAsync(Array.Empty<string>());
            Assert.Equal(1, result);
        }

        [Fact]
        public void InspectStreamFormat_JsonAndYamlDetection()
        {
            var app = new OverlayCliApp();

            // JSON stream
            using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{"));
            var jsonFormat = typeof(OverlayCliApp).GetMethod("InspectStreamFormat", BindingFlags.NonPublic | BindingFlags.Instance);
            var resultJson = (string)jsonFormat.Invoke(app, new object[] { jsonStream });
            Assert.Equal("json", resultJson);

            // YAML stream
            using var yamlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("openapi: 3.0.0"));
            var resultYaml = (string)jsonFormat.Invoke(app, new object[] { yamlStream });
            Assert.Equal("yaml", resultYaml);
        }
    }
}