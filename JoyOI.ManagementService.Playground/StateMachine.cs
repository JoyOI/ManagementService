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
        public override async Task RunAsync(string actor, BlobInfo[] blobs)
        {
            if (actor == null)
            {
                actor = "CompileUserCode";
            }

            switch (actor)
            {
                case "CompileUserCode":
                    await DeployAndRunActorAsync("CompileUserCodeActor", blobs);
                    goto case "RunUserCodeActor";
                case "RunUserCodeActor":
                    await DeployAndRunActorAsync("RunUserCodeActor", FinishedActors.Last().Outputs);
                    break;
            }
        }
    }
}
