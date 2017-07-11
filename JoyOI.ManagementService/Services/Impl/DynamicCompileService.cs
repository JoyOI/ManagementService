using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services.Impl
{
    internal class DynamicCompileService : IDynamicCompileService
    {
        private ConcurrentDictionary<string, byte[]> _compileCache { get; set; }
        private volatile object _assemblyLoadHelper;

        public DynamicCompileService()
        {
            _compileCache = new ConcurrentDictionary<string, byte[]>();
            _assemblyLoadHelper = typeof(Task).GetType();
            _assemblyLoadHelper = typeof(Process).GetType();
            _assemblyLoadHelper = typeof(JsonConvert).GetType();
        }

        public byte[] Compile(string code, OutputKind outputKind)
        {
            // 从缓存获取
            if (_compileCache.TryGetValue(code, out var assemblyBytes))
            {
                return assemblyBytes;
            }
            // 编译程序集
            var assemblyName = $"__DynamicAssembly_{DateTime.UtcNow.Ticks}";
            var optimizationLevel = OptimizationLevel.Debug;
            var compilationOptions = new CSharpCompilationOptions(
                outputKind,
                optimizationLevel: optimizationLevel);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !a.FullName.StartsWith("__"))
                .Select(a => MetadataReference.CreateFromFile(a.Location));
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(compilationOptions)
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTree);
            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);
                if (!emitResult.Success)
                {
                    throw new InvalidOperationException(string.Join("\r\n",
                        emitResult.Diagnostics.Where(d => d.WarningLevel == 0)));
                }
                assemblyBytes = stream.ToArray();
            }
            // 保存到缓存
            _compileCache[code] = assemblyBytes;
            return assemblyBytes;
        }
    }
}
