using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Configuration;

namespace MightyCalc.Configuration
{
    public static class Configuration
    {
        public static Config GetEnvironmentConfig(IDictionary<string, string> envNameAndPaths)
        {
            var stringBuilder = new StringBuilder();
            foreach (var envNameAndPath in envNameAndPaths)
            {
                var environmentVariableName = envNameAndPath.Key;
                var configPath = envNameAndPath.Value;

                var envValue = Environment.GetEnvironmentVariable(environmentVariableName);
                if (envValue == null) continue;
                stringBuilder.AppendLine(configPath + "=" + envValue);
            }

            Config customCfg = stringBuilder.ToString();

            return customCfg;
        }

        public static Config GetEnvironmentConfig(params string[] additionalVars)
        {
            var envNameAndPaths = new Dictionary<string, string>
            {
                {"MightyCalc_Journal", "akka.persistence.journal.postgresql.connection-string"},
                {"MightyCalc_SnapshotStore", "akka.persistence.snapshot-store.postgresql.connection-string"},
                {"MightyCalc_SeedNodes", "akka.cluster.seed-nodes"},
                {"MightyCalc_NodeRoles", "akka.cluster.roles"},
                {"MightyCalc_NodePort", "akka.remote.dot-netty.tcp.port"},
                {"MightyCalc_PublicHostName", "akka.remote.dot-netty.tcp.public-hostname"},
                {"MightyCalc_PublicIP", "akka.remote.dot-netty.tcp.public-ip"},
                {"MightyCalc_HostName", "akka.remote.dot-netty.tcp.hostname"},
                {"MightyCalc_CmdPort", "petabridge.cmd.port"},
                {"MightyCalc_CmdHost", "petabridge.cmd.host"},
            };

            if (additionalVars.Any())
            {
                if (additionalVars.Length % 2 != 0)
                    throw new ArgumentException("additionalVars should defines pairs of env variable and hocon paths");

                var enumerator = additionalVars.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var envVariable = enumerator.Current as string ??
                                      throw new ArgumentException("Environment variable name cannot be null");
                    enumerator.MoveNext();
                    var hoconPath = enumerator.Current as string ??
                                    throw new ArgumentException("Hocon path cannot be null");

                    envNameAndPaths[envVariable] = hoconPath;
                }
            }

            return GetEnvironmentConfig(envNameAndPaths);
        }
    }
}