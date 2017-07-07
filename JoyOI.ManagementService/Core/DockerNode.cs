using Docker.DotNet;
using Docker.DotNet.X509;
using JoyOI.ManagementService.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 单个Docker节点
    /// </summary>
    internal class DockerNode : IDisposable
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 节点信息
        /// </summary>
        public JoyOIManagementConfiguration.Node NodeInfo { get; private set; }
        /// <summary>
        /// 正在运行的任务数量
        /// </summary>
        public int RunningJobs { get; internal set; }

        private CertificateCredentials _credentials;
        private DockerClientConfiguration _dockerClientConfiguration;

        public DockerNode(string name, JoyOIManagementConfiguration.Node nodeInfo)
        {
            Name = name;
            NodeInfo = nodeInfo;
            _credentials = new CertificateCredentials(
                new X509Certificate2(nodeInfo.ClientCertificatePath, nodeInfo.ClientCertificatePassword));
            _dockerClientConfiguration = new DockerClientConfiguration(new Uri(nodeInfo.Address), _credentials);
        }

        public void Dispose()
        {
            _credentials.Dispose();
            _dockerClientConfiguration.Dispose();
        }

        public DockerClient CreateDockerClient()
        {
            return _dockerClientConfiguration.CreateClient();
        }
    }
}
