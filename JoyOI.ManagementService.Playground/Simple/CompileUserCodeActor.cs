using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JoyOI.ManagementService.Playground
{
    class CompileUserCodeActor
    {
        static void Main_(string[] args)
        {
            var p = Process.Start(new ProcessStartInfo("runner")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            p.StandardInput.WriteLine("5000");
            p.StandardInput.WriteLine("gcc Main.c -o Main.out");

            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                var error = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                throw new Exception(error);
            }

            var runnerInfo = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("runner.json"));
            if (runnerInfo["ExitCode"].Value<int>() != 0)
            {
                throw new InvalidOperationException(runnerInfo["Error"].Value<string>());
            }

            var json = JsonConvert.SerializeObject(new
            {
                Outputs = new string[] { "runner.json", "Main.out", "stdout.txt", "stderr.txt" }
            });
            File.WriteAllText("return.json", json);
        }
    }
}
