using System;
using System.Collections.Generic;
using System.Text;
using Akka.Configuration;

namespace MightyCalc.Configuration
{
    public class Configuration
    {
        public static Config GetEnvironmentConfig(IDictionary<string,string> envNameAndPaths)
        {
            var stringBuilder = new StringBuilder();
            foreach (var envNameAndPath in envNameAndPaths)
            {
                var environmentVariableName = envNameAndPath.Key;
                var configPath = envNameAndPath.Value;

                var envValue = Environment.GetEnvironmentVariable(environmentVariableName);
                if (envValue == null) continue;
                stringBuilder.AppendLine(configPath+"="+envValue);
            }

            Config customCfg = stringBuilder.ToString();
	        
            return customCfg;
        }
    }
}