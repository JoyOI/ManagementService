using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace JoyOI.ManagementService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    var configuration = Startup._kestrelConfiguration;
                    if (configuration != null)
                    {
                        options.Listen(IPAddress.Any, configuration.HttpsListenPort, listenOptions =>
                        {
                            var httpsOptions = new HttpsConnectionAdapterOptions();
                            httpsOptions.ServerCertificate = new X509Certificate2(
                                configuration.ServerCertificatePath, configuration.ServerCertificatePassword);
                            httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                            httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
                            {
                                // 检查客户端证书
                                // return cert.Equals(clientCertificate);
                                return chain.Build(cert);
                            };
                            httpsOptions.SslProtocols = SslProtocols.Tls12;
                            listenOptions.UseHttps(httpsOptions);
                        });
                    }
                })
                .UseIISIntegration()
                .Build();
            host.Run();
        }
    }
}
