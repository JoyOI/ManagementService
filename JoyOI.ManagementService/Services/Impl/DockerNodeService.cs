using System;
using System.Collections.Generic;
using System.Text;
using JoyOI.ManagementService.Model.Dtos;
using System.Linq;
using AutoMapper;
using JoyOI.ManagementService.Core;
using Microsoft.Extensions.DependencyInjection;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 获取Docker节点的服务, 对外提供
    /// </summary>
    public class DockerNodeService : IDockerNodeService
    {
        private IDockerNodeStore _store;

        public DockerNodeService(IServiceProvider provider)
            : this(provider.GetRequiredService<IDockerNodeStore>()) { }

        internal DockerNodeService(IDockerNodeStore store)
        {
            _store = store;
        }

        public IEnumerable<DockerNodeOutputDto> GetNodes()
        {
            var nodes = _store.GetNodes()
                .Select(x => Mapper.Map<DockerNode, DockerNodeOutputDto>(x))
                .ToList();
            return nodes;
        }

        public IDictionary<int, int> GetWaitingTasks()
        {
            return _store.GetWaitingTasks();
        }
    }
}
