using AutoMapper;
using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Services.Impl;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.FunctionalTests.Services
{
    public abstract class TestServiceBase : IDisposable
    {
        protected JoyOIManagementConfiguration _configuration { get; set; }
        protected DummyStorage _storage;

        public TestServiceBase()
        {
            var dir = Environment.CurrentDirectory;
            while (!File.Exists(Path.Combine(dir, "appsettings.json")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Environment.CurrentDirectory = dir;
            JoyOIManagementServiceCollectionExtensions.InitializeStaticFunctions();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            _configuration = new JoyOIManagementConfiguration();
            configuration.GetSection("JoyOIManagement").Bind(_configuration);
            _configuration.AfterLoaded();
            _storage = new DummyStorage();
            // 数据库除错代码
            if (false)
            {
                _storage.OnSaveChanges = (storage) =>
                {
                    var stackTrace = new StackTrace().ToString();
                    var tables = JsonConvert.SerializeObject(storage.Tables, Formatting.Indented);
                    File.AppendAllText($"e:\\mgmtsvc_dblog_{Process.GetCurrentProcess().Id}.txt",
                        $"{stackTrace}\r\n{tables}\r\n=====================================================\r\n\r\n");
                };
            }
        }

        public virtual void Dispose()
        {
        }

        protected async Task<Guid> PutActor(string name, string body)
        {
            var repository = new DummyRepository<ActorEntity, Guid>(_storage);
            var service = new ActorService(repository);
            return await service.Put(new ActorInputDto()
            {
                Name = name,
                Body = body
            });
        }

        protected async Task<Guid> PutStateMachine(string name, string body)
        {
            var repository = new DummyRepository<StateMachineEntity, Guid>(_storage);
            var service = new StateMachineService(repository);
            return await service.Put(new StateMachineInputDto()
            {
                Name = name,
                Body = body
            });
        }

        protected async Task<Guid> PutBlob(string remark, byte[] body)
        {
            var repository = new DummyRepository<BlobEntity, Guid>(_storage);
            var service = new BlobService(repository);
            return await service.Put(new BlobInputDto()
            {
                TimeStamp = Mapper.Map<DateTime, long>(DateTime.UtcNow),
                Body = Mapper.Map<byte[], string>(body),
                Remark = remark
            });
        }

        protected async Task<StateMachineInstancePutDto> PutSimpleDataSet()
        {
            await PutActor("CompileUserCodeActor", @"
                using Newtonsoft.Json;
                using Newtonsoft.Json.Linq;
                using System;
                using System.Diagnostics;
                using System.IO;

                namespace JoyOI.ManagementService.Playground
                {
                    class CompileUserCodeActor
                    {
                        static void Main(string[] args)
                        {
                            var p = Process.Start(new ProcessStartInfo(""runner"") { RedirectStandardInput = true });
                            p.StandardInput.WriteLine(""5000"");
                            p.StandardInput.WriteLine(""gcc Main.c -o Main.out"");
                            p.WaitForExit();

                            var runnerInfo = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(""runner.json""));
                            if (runnerInfo[""ExitCode""].Value<int>() != 0)
                            {
                                throw new InvalidOperationException(File.ReadAllText(""stderr.txt""));
                            }

                            var json = JsonConvert.SerializeObject(new
                            {
                                Outputs = new string[] { ""runner.json"", ""Main.out"", ""stdout.txt"", ""stderr.txt"" }
                            });
                            File.WriteAllText(""return.json"", json);
                        }
                    }
                }");
            await PutActor("RunUserCodeActor", @"
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
                            Process.Start(""chmod"", ""+x Main.out"").WaitForExit();
                            var p = Process.Start(new ProcessStartInfo(""runner"") { RedirectStandardInput = true });
                            p.StandardInput.WriteLine(""5000"");
                            p.StandardInput.WriteLine(""./Main.out"");
                            p.WaitForExit();
                            var json = JsonConvert.SerializeObject(new
                            {
                                Outputs = new string[] { ""runner.json"", ""stdout.txt"", ""stderr.txt"" }
                            });
                            File.WriteAllText(""return.json"", json);
                        }
                    }
                }");
            await PutStateMachine("SimpleStateMachine", @"
                using JoyOI.ManagementService.Core;
                using System;
                using System.Collections.Generic;
                using System.Text;
                using Microsoft.EntityFrameworkCore.Migrations;
                using System.Threading.Tasks;

                namespace JoyOI.ManagementService.Playground
                {
                    public class SimpleStateMachine : StateMachineBase
                    {
                        public override async Task RunAsync()
                        {
                            switch (Stage)
                            {
                                case ""Start"":
                                    goto case ""CompileUserCode"";
                                case ""CompileUserCode"":
                                    await SetStageAsync(""CompileUserCode"");
                                    await DeployAndRunActorAsync(new RunActorParam(""CompileUserCodeActor"", InitialBlobs));
                                    goto case ""RunUserCode"";
                                case ""RunUserCode"":
                                    await SetStageAsync(""RunUserCode"");
                                    var compileActorInfo = StartedActors.FindSingleActor(actor: ""CompileUserCodeActor"");
                                    await DeployAndRunActorAsync(new RunActorParam(""RunUserCodeActor"", compileActorInfo.Outputs));
                                    break;
                            }
                        }
                    }
                }");
            var blobId = await PutBlob("Main.c", Encoding.UTF8.GetBytes(@"
                #include <stdio.h>
                int main() {
                    printf(""simple state machine is ok\r\n"");
                }"));
            return new StateMachineInstancePutDto()
            {
                Name = "SimpleStateMachine",
                InitialBlobs = new[]
                {
                    new BlobInfo(blobId, "Main.c")
                }
            };
        }
    }
}
