using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace JoyOI.ManagementService.Playground
{
    class Actor
    {
        static void Main(string[] args)
        {
            var p = Process.Start("runner");
            p.StandardInput.WriteLine("5000");
            p.StandardInput.WriteLine("gcc Main.c -o Main.out");
            p.WaitForExit();
            var json = JsonConvert.SerializeObject(new
            {
                Outputs = new string[] { "runner.json", "Main.out", "stdout.txt", "stderr.txt" }
            });
            File.WriteAllText("return.json", json);
        }
    }
}
