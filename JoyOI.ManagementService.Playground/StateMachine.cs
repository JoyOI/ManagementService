using JoyOI.ManagementService.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;

/*

测试参数:

{
  "name": "CompileAndRun",
  "initialBlobs": [
    {
      "id": "填写blob.id",
      "name": "Main.c"
    }
  ]
}

*/

namespace JoyOI.ManagementService.Playground
{
    public class StateMachine : StateMachineBase
    {
        public static Regex InputFileRegex = new Regex("input_[0-9]{0,}.txt");
        public static Regex OutputFileRegex = new Regex("output_[0-9]{0,}.txt");

        public override async Task RunAsync()
        {
            // Prepare
            var profile = await InitialBlobs.FindBlob("profile.json").ReadAsJsonAsync<dynamic>(this);

            switch (Stage)
            {
                case "Start":
                    await SetStageAsync("Start");
                    await DeployAndRunActorsAsync(
                        new RunActorParam("CompileUserCodeActor", InitialBlobs.FindBlob("Main.cpp")), 
                        new RunActorParam("CompileValidatorCodeActor", InitialBlobs.FindBlob("Validator.cpp")));
                    goto case "ValidateUserCompileResult";
                case "ValidateUserCompileResult":
                    await SetStageAsync("ValidateUserCompileResult");
                    var json = await StartedActors.FindSingleActor("Start", "CompileUserCodeActor").Outputs.FindBlob("runner.json").ReadAsJsonAsync<dynamic>(this);
                    if (json.ExitCode != 0)
                    {
                        if (json.IsTimeout)
                        {
                            await HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = "Compiler timeout.",
                                TimeUsed = json.UserTime
                            });
                        }
                        else
                        {
                            await HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = StartedActors
                                    .FindSingleActor("Start", "CompileUserCodeActor")
                                    .Outputs
                                    .FindBlob("stderr.txt"),
                                ExitCode = json.ExitCode
                            });
                        }
                        break;
                    }
                    goto case "ValidateValidatorCompileResult";
                case "ValidateValidatorCompileResult":
                    await SetStageAsync("ValidateValidatorCompileResult");
                    var json2 = await StartedActors
                        .FindSingleActor("Start", "CompileValidatorCodeActor")
                        .Outputs
                        .FindBlob("runner.json")
                        .ReadAsJsonAsync<dynamic>(this);
                    if (json2.ExitCode != 0)
                    {
                        if (json2.IsTimeout)
                        {
                            await HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = "Validator Compile Error",
                                Error = "Compiler timeout.",
                                TimeUsed = json2.UserTime
                            });
                        }
                        else
                        {
                            await HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = "Validator Compile Error",
                                Error = await StartedActors
                                    .FindSingleActor("Start", "CompileValidatorCodeActor")
                                    .Outputs
                                    .FindBlob("stderr.txt")
                                    .ReadAllTextAsync(this),
                                ExitCode = json2.ExitCode
                            });
                        }
                        break;
                    }
                    goto case "RunUserProgramActor";
                case "RunUserProgramActor":
                    await SetStageAsync("RunUserProgramActor");
                    var compileUserCodeActor = StartedActors.FindSingleActor("Start", "CompileUserCodeActor");
                    var json3 = await compileUserCodeActor.Outputs.FindBlob("runner.json").ReadAsJsonAsync<dynamic>(this);
                    if (json3.ExitCode != 0)
                    {
                        if (json3.IsTimeout)
                        {
                            await HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = "Compiler timeout.",
                                TimeUsed = json3.UserTime
                            });
                        }
                        else
                        {
                            await HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = "Compile Error",
                                Error = await compileUserCodeActor
                                    .Outputs
                                    .FindBlob("stderr.txt").ReadAllTextAsync(this),
                                ExitCode = json3.ExitCode
                            });
                        }
                        break;
                    }
                    var runActorParams = new List<RunActorParam>();
                    var inputs = InitialBlobs.Where(x => InputFileRegex.IsMatch(x.Name));
                    var userProgram = StartedActors.FindSingleActor("Start", "CompileUserCodeActor").Outputs.FindBlob("Main.out");
                    foreach (var x in inputs)
                    {
                        runActorParams.Add(new RunActorParam("RunUserProgramActor", x, userProgram));
                    }
                    await DeployAndRunActorsAsync(runActorParams.ToArray());
                    goto case "ValidateUserOutput";
                case "ValidateUserOutput":
                    await SetStageAsync("ValidateUserOutput");
                    var runUserProgramActors = StartedActors.FindActor("RunUserProgramActor");
                    var tasks4 = new List<Task>();
                    foreach (var x in runUserProgramActors)
                    {
                        var json4 = await x.Outputs.FindBlob("runner.json").ReadAsJsonAsync<dynamic>(this);
                        if (json4.PeakMemory > profile.Memory)
                        {
                            tasks4.Add(HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = "Memory Limit Exceeded",
                                InputFile = x.Inputs.Single(y => InputFileRegex.IsMatch(y.Name))
                            }));
                        }
                        else if (json4.ExitCode != 0)
                        {
                            tasks4.Add(HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + this.Id, new
                            {
                                Result = json4.IsTimeout ? "Time Limit Exceeded" : "Runtime Error",
                                InputFile = x.Inputs.Single(y => InputFileRegex.IsMatch(y.Name))
                            }));
                        }
                        else
                        {
                            var answerFilename = x.Outputs.Single(y => InputFileRegex.IsMatch(y.Name)).Name.Replace("input_", "output_");
                            var answer = InitialBlobs.FindBlob(answerFilename);
                            var stdout = x.Outputs.Single(y => y.Name == "stdout.txt");
                            var validator = StartedActors
                                .FindSingleActor("Start", "CompileValidatorCodeActor")
                                .Outputs
                                .FindBlob("Validator.out");
                            tasks4.Add(DeployAndRunActorAsync(new RunActorParam("CompareActor", new[] { answer, stdout, validator })));
                        }
                    }
                    await Task.WhenAll(tasks4);
                    goto case "Finally";
                case "Finally":
                    await SetStageAsync("Finally");
                    var compileActors = StartedActors.FindActor("ValidateUserOutput", "CompareActor");
                    var tasks5 = new List<Task>();
                    foreach (var x in compileActors)
                    {
                        var json5 = await x.Outputs.FindBlob("runner.json").ReadAsJsonAsync<dynamic>(this);
                        tasks5.Add(HttpInvokeAsync(HttpMethod.Put, "/JudgeResult/" + Id, new
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