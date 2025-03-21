using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using WeChatMomentSimulator.Core.Interfaces;

namespace WeChatMomentSimulator.Services.Infrastructure
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly string _environment;
        private const string DevelopmentFlag = "development.flag";
        private const string EnvVarName = "WCMSIMULATOR_ENVIRONMENT";
        
        public EnvironmentService(string[] args, IConfiguration configuration)
        {
            // 检查顺序: 命令行参数 > 环境变量 > 配置文件 > 标志文件 > 默认(Production)
            
            // 1. 检查命令行参数
            for (int i = 0; i < args.Length - 1; i++)
            {
                if ((args[i] == "--env" || args[i] == "--environment") && args[i + 1].Equals("Development", StringComparison.OrdinalIgnoreCase))
                {
                    _environment = "Development";
                    return;
                }
            }
            
            // 2. 检查环境变量
            string envVar = Environment.GetEnvironmentVariable(EnvVarName);
            if (!string.IsNullOrEmpty(envVar) && envVar.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                _environment = "Development";
                return;
            }
            
            // 3. 检查配置文件
            string configEnv = configuration["Environment"];
            if (!string.IsNullOrEmpty(configEnv) && configEnv.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                _environment = "Development";
                return;
            }
            
            // 4. 检查开发标志文件
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(Path.Combine(appDir, DevelopmentFlag)))
            {
                _environment = "Development";
                return;
            }
            
            // 5. 默认为生产环境
            _environment = "Production";
        }

        public bool IsDevelopment()
        {
            return _environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        public string GetCurrentEnvironment()
        {
            return _environment;
        }
    }
}