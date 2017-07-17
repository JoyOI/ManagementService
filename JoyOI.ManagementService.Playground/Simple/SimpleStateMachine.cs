using JoyOI.ManagementService.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;

/*
测试参数:
{
  "name": "SimpleStateMachine",
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
    public class SimpleStateMachine : StateMachineBase
    {
        public override async Task RunAsync()
        {
            switch (Stage)
            {
                case "Start":
                    goto case "CompileUserCode";
                case "CompileUserCode":
                    await SetStageAsync("CompileUserCode");
                    await DeployAndRunActorAsync(new RunActorParam("CompileUserCodeActor", InitialBlobs));
                    goto case "RunUserCode";
                case "RunUserCode":
                    await SetStageAsync("RunUserCode");
                    var compileActorInfo = StartedActors.FindSingleActor(actor: "CompileUserCodeActor");
                    await DeployAndRunActorAsync(new RunActorParam("RunUserCodeActor", compileActorInfo.Outputs));
                    break;
            }
        }
    }
}
