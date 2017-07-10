﻿using Newtonsoft.Json;
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
            var p = Process.Start(new ProcessStartInfo("runner") { RedirectStandardInput = true });
            p.StandardInput.WriteLine("5000");
            p.StandardInput.WriteLine("./Main.out");
            p.WaitForExit();
            var json = JsonConvert.SerializeObject(new
            {
                Outputs = new string[] { "runner.json", "stdout.txt", "stderr.txt" }
            });
            File.WriteAllText("return.json", json);
        }
    }
}