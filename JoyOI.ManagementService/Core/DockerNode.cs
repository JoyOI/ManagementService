using Docker.DotNet;
using Docker.DotNet.X509;
using JoyOI.ManagementService.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 单个Docker节点
    /// </summary>
    internal class DockerNode : IDisposable
    {
        public JoyOIManagementConfiguration.Node NodeInfo { get; private set; }
        private CertificateCredentials _credentials;
        private DockerClientConfiguration _dockerClientConfiguration;

        public DockerNode(JoyOIManagementConfiguration.Node nodeInfo)
        {
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
    }
}
