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
        public class HoconValueProvider
        {
            public HoconValueProvider(string path, string environmentVariableName,
                Func<string, string> formatter = null)
            {
                Path = path;
                EnvironmentVariableName = environmentVariableName;
                _formatter = formatter;
            }

            public string Path { get; }
            public string EnvironmentVariableName { get; }
            private readonly Func<string, string> _formatter;

            public string GetValue()
            {
                var variable = Environment.GetEnvironmentVariable(EnvironmentVariableName);
                var hoconValue = variable == null ? null : _formatter(variable);
                return hoconValue;
            }

            public string GetHocon()
            {
                var value = GetValue();
                return value == null ? string.Empty : $"{Path} = {value}";
            }

            public static HoconValueProvider Quoted(string env, string path) =>
                new HoconValueProvider(path, env, s => string.IsNullOrWhiteSpace(s) ? "\"\"" : $"\"{s}\"");

            public static HoconValueProvider Array(string env, string path) =>
                new HoconValueProvider(path, env, s => string.IsNullOrWhiteSpace(s) ? "[]" : 
                    $"[{string.Join(",", s.Split(' ').Select(ss => $"\"{ss}\""))}]");
        }

        public static Config GetEnvironmentConfig(params HoconValueProvider[] additionalProviders)
        {
            var defaultProviders = new[]
            {
                HoconValueProvider.Quoted("MightyCalc_Journal",
                    "akka.persistence.journal.postgresql.connection-string"),
                HoconValueProvider.Quoted("MightyCalc_SnapshotStore",
                    "akka.persistence.snapshot-store.postgresql.connection-string"),
                HoconValueProvider.Array("MightyCalc_SeedNodes", "akka.cluster.seed-nodes"),
                HoconValueProvider.Array("MightyCalc_NodeRoles", "akka.cluster.roles"),
                HoconValueProvider.Quoted("MightyCalc_NodePort", "akka.remote.dot-netty.tcp.port"),
                HoconValueProvider.Quoted("MightyCalc_PublicHostName",
                    "akka.remote.dot-netty.tcp.public-hostname"),
                HoconValueProvider.Quoted("MightyCalc_PublicIP", "akka.remote.dot-netty.tcp.public-ip"),
                HoconValueProvider.Quoted("MightyCalc_HostName", "akka.remote.dot-netty.tcp.hostname"),
                HoconValueProvider.Quoted("MightyCalc_CmdPort", "petabridge.cmd.port"),
                HoconValueProvider.Quoted("MightyCalc_CmdHost", "petabridge.cmd.host")
            };

            var providers = additionalProviders.Any() ? defaultProviders.Concat(additionalProviders) : defaultProviders;
            var stringBuilder = new StringBuilder();

            providers.Select(p => p.GetHocon())
                     .Where(h => h != null)
                     .ToList()
                     .ForEach(h => stringBuilder.AppendLine(h));
            
            Config customCfg = stringBuilder.ToString();

            return customCfg;
        }
    }
}