using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Akka.Configuration;

namespace MightyCalc.Configuration
{
    public static class Configuration
    {
        public class HoconConfigProvider
        {
            public HoconConfigProvider(string path, string environmentVariableName,
                Func<string, string> formatter = null)
            {
                Path = path;
                EnvironmentVariableName = environmentVariableName;
                _formatter = formatter;
            }

            public string Path { get; }
            public string EnvironmentVariableName { get; }
            private readonly Func<string, string> _formatter;

            public string GetHocon()
            {
                var variable = Environment.GetEnvironmentVariable(EnvironmentVariableName);
                var hoconValue = variable == null ? null : _formatter(variable);
                return $"{Path} = {hoconValue}";
            }

            public static HoconConfigProvider Quoted(string env, string path) =>
                new HoconConfigProvider(path, env, s => $"\"{s}\"");

            public static HoconConfigProvider Array(string env, string path) =>
                new HoconConfigProvider(path, env, s =>
                    $"[{string.Join(",", s.Split(' ').Select(ss => $"\"{ss}\""))}]");
        }

        public static Config GetEnvironmentConfig(params HoconConfigProvider[] additionalProviders)
        {
            var defaultProviders = new []
            {
                HoconConfigProvider.Quoted("MightyCalc_Journal",
                    "akka.persistence.journal.postgresql.connection-string"),
                HoconConfigProvider.Quoted("MightyCalc_SnapshotStore",
                    "akka.persistence.snapshot-store.postgresql.connection-string"),
                HoconConfigProvider.Array("MightyCalc_SeedNodes", "akka.cluster.seed-nodes"),
                HoconConfigProvider.Array("MightyCalc_NodeRoles", "akka.cluster.roles"),
                HoconConfigProvider.Quoted("MightyCalc_NodePort", "akka.remote.dot-netty.tcp.port"),
                HoconConfigProvider.Quoted("MightyCalc_PublicHostName",
                    "akka.remote.dot-netty.tcp.public-hostname"),
                HoconConfigProvider.Quoted("MightyCalc_PublicIP", "akka.remote.dot-netty.tcp.public-ip"),
                HoconConfigProvider.Quoted("MightyCalc_HostName", "akka.remote.dot-netty.tcp.hostname"),
                HoconConfigProvider.Quoted("MightyCalc_CmdPort", "petabridge.cmd.port"),
                HoconConfigProvider.Quoted("MightyCalc_CmdHost", "petabridge.cmd.host")
            };

            var providers = additionalProviders.Any() ? defaultProviders.Concat(additionalProviders) : defaultProviders; 
            var stringBuilder = new StringBuilder();
            
            foreach (var envNameAndPath in providers)
            {
                stringBuilder.AppendLine(envNameAndPath.GetHocon());
            }

            Config customCfg = stringBuilder.ToString();

            return customCfg;
        }
    }
}