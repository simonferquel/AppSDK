using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docker.WebStore
{
    public class BackendService
    {
        private readonly string _exePath;

        public BackendService(string exePath)
        {
            _exePath = exePath;
        }

        public async Task RunAsync(string name, string svcFile, string paramsFile, string runtimeFile)
        {
            var psi = new ProcessStartInfo(_exePath) {
                Arguments = $"deploy -n \"{name}\" -f \"{paramsFile}\" -c \"{runtimeFile}\" -",
                RedirectStandardInput = true,
            };
            var process = Process.Start(psi);
            using (var fs = File.OpenRead(svcFile)) {
                await fs.CopyToAsync(process.StandardInput.BaseStream);
                await process.StandardInput.BaseStream.FlushAsync();
                process.StandardInput.Close();
            }
            process.WaitForExit();
            if (process.ExitCode != 0) {
                throw new Exception("process has failed");
            }

        }
    }
}
