using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JoyOI.ManagementService.Playground
{
    class RunUserCodeActor
    {
        static void Main(string[] args)
        {
            Process.Start("chmod", "+x Main.out").WaitForExit();
            var p = Process.Start(new ProcessStartInfo("runner")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            p.StandardInput.WriteLine("5000");
            p.StandardInput.WriteLine("./Main.out");

            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                var error = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                throw new Exception(error);
            }

            var json = JsonConvert.SerializeObject(new
            {
                Outputs = new string[] { "runner.json", "stdout.txt", "stderr.txt" }
            });
            File.WriteAllText("return.json", json);
        }
    }
}
