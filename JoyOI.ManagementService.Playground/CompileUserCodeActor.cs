﻿using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace JoyOI.ManagementService.Playground
{
    class CompileUserCodeActor
    {
        static void Main_(string[] args)
        {
            var p = Process.Start(new ProcessStartInfo("runner") { RedirectStandardInput = true });
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