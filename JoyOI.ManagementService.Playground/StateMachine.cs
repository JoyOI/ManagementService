using JoyOI.ManagementService.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;
using System.Linq;

namespace JoyOI.ManagementService.Playground
{
    // Special Judge State Machine
    public class StateMachine : StateMachineBase
    {
        public static Regex InputFileRegex = new Regex("input_[0-9]{0,}.txt");
        public static Regex OutputFileRegex = new Regex("output_[0-9]{0,}.txt");

        public override async Task RunAsync(string state = "Start")
        {
            // Prepare
            var limitations = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ReadAllText(Blobs.Single(x => x.Name == "limitation.json")));

            switch (state)
            {
                case "Start":
                    SetState("Start");
                    await Task.WhenAll(
                        DeployAndRunActorAsync("CompileUserCodeActor", new[] { Blobs.Single(x => x.Name == "Main.cpp") }),
                        DeployAndRunActorAsync("CompileValidatorCodeActor", new[] { Blobs.Single(x => x.Name == "Validator.cpp") }));
                    goto case "ValidateUserCompileResult";
                case "ValidateUserCompileResult":
                    SetState("ValidateUserCompileResult");
                    var text = ReadAllText(FindSingleActor("Start", "CompileUserCodeActor").Outputs.Single(y => y.Name == "runner.json"));
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(text);
                    if (json.ExitCode != 0)
                    {
                        if (json.IsTimeout)
                        {
                            await HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = "Compiler timeout.",
                                TimeUsed = json.UserTime
                            });
                        }
                        else
                        {
                            await HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = ReadAllText(FindSingleActor("Start").Outputs.Single(x => x.Name == "stderr.txt")),
                                ExitCode = json.ExitCode
                            });
                        }
                        break;
                    }
                    goto case "ValidateValidatorCompileResult";
                case "ValidateValidatorCompileResult":
                    SetState("ValidateValidatorCompileResult");
                    var text2 = ReadAllText(FindSingleActor("Start", "CompileValidatorCodeActor").Outputs.Single(y => y.Name == "runner.json"));
                    var json2 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(text2);
                    if (json2.ExitCode != 0)
                    {
                        if (json2.IsTimeout)
                        {
                            await HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = "Validator Compile Error",
                                Error = "Compiler timeout.",
                                TimeUsed = json2.UserTime
                            });
                        }
                        else
                        {
                            await HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = "Validator Compile Error",
                                Error = ReadAllText(FindSingleActor("Start").Outputs.Single(x => x.Name == "stderr.txt")),
                                ExitCode = json2.ExitCode
                            });
                        }
                        break;
                    }
                    goto case "RunUserCodeActor";
                case "RunUserCodeActor":
                    SetState("RunUserCodeActor");
                    var text3 = ReadAllText(FindSingleActor("Start").Outputs.Single(y => y.Name == "runner.json"));
                    var json3 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(text3);
                    if (json3.ExitCode != 0)
                    {
                        if (json3.IsTimeout)
                        {
                            await HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = "Compiler timeout.",
                                TimeUsed = json3.UserTime
                            });
                        }
                        else
                        {
                            await HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = ReadAllText(FindSingleActor("Start").Outputs.Single(x => x.Name == "stderr.txt")),
                                ExitCode = json3.ExitCode
                            });
                        }
                        break;
                    }
                    var tasks = new List<Task>();
                    var inputs = Blobs.Where(x => InputFileRegex.IsMatch(x.Name));
                    var userProgram = FindSingleActor("Start").Outputs.Single(x => x.Name == "Main.out");
                    foreach (var x in inputs)
                    {
                        tasks.Add(DeployAndRunActorAsync("RunUserCodeActor", new[]
                        {
                            userProgram,
                            x
                        }));
                    }
                    await Task.WhenAll(tasks);
                    goto case "ValidateUserOutput";
                case "ValidateUserOutput":
                    SetState("ValidateUserOutput");
                    var actors = FindActor("RunUserCodeActor");
                    var tasks4 = new List<Task>();
                    foreach (var x in actors)
                    {
                        var text4 = ReadAllText(x.Outputs.Single(y => y.Name == "runner.json"));
                        var json4 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(text4);
                        if (json4.PeakMemory > limitations.Memory)
                        {
                            tasks4.Add(HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = "Memory Limit Exceeded",
                                InputFile = x.Inputs.Single(y => InputFileRegex.IsMatch(y.Name))
                            }));
                        }
                        else if (json4.ExitCode != 0)
                        {
                            tasks4.Add(HttpInvokeAsync("PUT", "/JudgeResult/" + this.Id, new
                            {
                                Result = json4.IsTimeout ? "Time Limit Exceeded" : "Runtime Error",
                                InputFile = x.Inputs.Single(y => InputFileRegex.IsMatch(y.Name))
                            }));
                        }
                        else
                        {
                            var answerFilename = x.Outputs.Single(y => InputFileRegex.IsMatch(y.Name)).Name.Replace("input_", "output_");
                            var answer = Blobs.Single(y => y.Name == answerFilename);
                            var stdout = x.Outputs.Single(y => y.Name == "stdout.txt");
                            var validator = FindSingleActor("Start", "CompileValidatorCodeActor").Outputs.Single(y => y.Name == "Validator.out");
                            tasks4.Add(DeployAndRunActorAsync("CompareActor", new[] { answer, stdout, validator }));
                        }
                    }
                    await Task.WhenAll(tasks4);
                    goto case "Finally";
                case "Finally":
                    SetState("Finally");
                    var actors5 = FindActor("ValidateUserOutput", "CompareActor");
                    var tasks5 = new List<Task>();
                    foreach (var x in actors5)
                    {
                        var text5 = ReadAllText(x.Outputs.Single(y => y.Name == "runner.json"));
                        var json5 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(text5);
                        tasks5.Add(HttpInvokeAsync("PUT", "/JudgeResult/" + Id, new
                        {
                            Result = json5.ExitCode == 0 ? "Accepted" : (json5.ExitCode == 1 ? "Wrong Answer" : (json5.ExitCode == 2 ? "Presentation Error" : "Validator Error")),
                            TimeUsed = json5.UserTime,
                            MemoryUsed = json5.PeakMemory,
                            InputFile = x.Inputs.Single(y => OutputFileRegex.IsMatch(y.Name)).Name.Replace("output_", "input_")
                        }));
                    }
                    await Task.WhenAll(tasks5);
                    break;
            }
        }
    }
}
