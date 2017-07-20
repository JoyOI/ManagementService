using Docker.DotNet;
using Docker.DotNet.X509;
using JoyOI.ManagementService.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
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
        /// <summary>
        /// 上次使用此节点执行任务是否出错
        /// </summary>
        public bool ErrorFlags { get; internal set; }
        /// <summary>
        /// Docker客户端对象
        /// 内部使用了HttpClient, 可以单例使用
        /// </summary>
        public DockerClient Client => _client;

        private CertificateCredentials _credentials;
        private DockerClientConfiguration _dockerClientConfiguration;
        private DockerClient _client;

        public DockerNode(string name, JoyOIManagementConfiguration.Node nodeInfo)
        {
            Name = name;
            NodeInfo = nodeInfo;
            _credentials = new CertificateCredentials(
                new X509Certificate2(nodeInfo.ClientCertificatePath, nodeInfo.ClientCertificatePassword));
            _dockerClientConfiguration = new DockerClientConfiguration(new Uri(nodeInfo.Address), _credentials);
            _client = _dockerClientConfiguration.CreateClient();
        }

        public void Dispose()
        {
            _credentials.Dispose();
            _dockerClientConfiguration.Dispose();
        }

        internal static bool IsConnectionError(Exception ex)
        {
            return ex is HttpRequestException || ex is SocketException;
        }
    }
}
