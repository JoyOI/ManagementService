using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Services;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 状态机实例的基础类
    /// </summary>
    public abstract class StateMachineBase : IDisposable
    {
        /// <summary>
        /// 状态机ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 状态机健康状态
        /// </summary>
        public StateMachineStatus Status { get; set; }

        /// <summary>
        /// 全部Actor列表
        /// </summary>
        public IList<ActorInfo> Actors { get; set; }

        /// <summary>
        /// 状态机状态
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// 状态机实例仓库
        /// </summary>
        internal IStateMachineInstanceStore Store { get; set; }

        /// <summary>
        /// 未知
        /// </summary>
        internal ContainerLimitation Limitation { get; set; }

        /// <summary>
        /// 当前状态机所使用的文件
        /// </summary>
        public IList<BlobInfo> Blobs { get; private set; } = new List<BlobInfo>();

        /// <summary>
        /// 创建新的StateMachine时需提供初始文件
        /// </summary>
        /// <param name="blobs"></param>
        public StateMachineBase(IEnumerable<BlobInfo> blobs = null)
        {
            if (blobs != null)
            {
                foreach (var x in blobs)
                {
                    Blobs.Add(x);
                }
            }
        }

        /// <summary>
        /// 在状态机运行完毕时执行
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="state">新状态</param>
        public void SetState(string state)
        {
            if (Actors.Any(x => x.Status == ActorStatus.Running))
            {
                var actors = Actors.Where(x => x.Status == ActorStatus.Running).ToList();
                throw new InvalidOperationException($"The state {State} is still in running, cannot set into a new state.", new Exception(JsonConvert.SerializeObject(actors)));
            }

            foreach (var x in Actors.Where(x => x.State == state))
            {
                Actors.Remove(x);
            }

            State = state;
        }

        protected async Task<ActorInfo> DeployAndRunActorAsync(string actor, IEnumerable<BlobInfo> inputs)
        {
            var _id = Guid.NewGuid();
            var _inputs = new List<BlobInfo>();
            foreach (var x in inputs)
            {
                var y = x;
                y.ActorId = _id;
                y.State = State;
                Blobs.Add(y);
                _inputs.Add(y);
            }

            // 创建新的CurrentActor
            var _actor = new ActorInfo()
            {
                Id = _id,
                Name = actor,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                Inputs = _inputs,
                Outputs = new BlobInfo[0],
                Exceptions = new string[0],
                Status = ActorStatus.Running,
                State = State
            };
            Actors.Add(_actor);

            // 传到docker上执行
            await Store.RunActor(this);

            // TODO: 下载相应文件，存储到数据库，并向Blobs添加
            // DBNull.Blobs.Add();
            // Blobs.Add();

            _actor.Status = ActorStatus.Succeeded;

            return _actor;
        }

        protected IEnumerable<ActorInfo> FindActor(string state = null, string actor = null)
        {
            IEnumerable<ActorInfo> ret = Actors;

            if (actor != null)
            {
                ret = ret.Where(x => x.Name == actor);
            }

            if (state != null)
            {
                ret = ret.Where(x => x.State == state);
            }

            return ret;
        }

        protected ActorInfo FindSingleActor(string state = null, string actor = null) => FindActor(state, actor).Single();

        public abstract Task RunAsync(string state = "Start");

        public string ReadAllText(BlobInfo blobInfo)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadAllBytes(BlobInfo blobInfo)
        {
            throw new NotImplementedException();
        }

        public Task<string> HttpInvokeAsync(string method, string endpoint, object body)
        {
            throw new NotImplementedException();
        }
    }
}
