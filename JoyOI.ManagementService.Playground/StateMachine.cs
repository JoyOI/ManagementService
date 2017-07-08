using JoyOI.ManagementService.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;
using System.Linq;

namespace JoyOI.ManagementService.Playground
{
    public class StateMachine : StateMachineBase
    {
        public override async Task RunAsync()
        {
            switch (Stage)
            {
                case "Start":
                    goto case "CompileUserCode";
                case "CompileUserCode":
                    await SetStage("CompileUserCode");
                    await DeployAndRunActorAsync(new RunActorParam("CompileUserCodeActor", InitialBlobs));
                    goto case "RunUserCode";
                case "RunUserCode":
                    await SetStage("RunUserCode");
                    var compileActorInfo = FindSingleActor(actor: "CompileUserCodeActor");
                    await DeployAndRunActorAsync(new RunActorParam("RunUserCodeActor", compileActorInfo.Outputs));
                    break;
            }
        }
    }
}
