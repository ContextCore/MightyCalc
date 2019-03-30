using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Serialization;

namespace MightyCalc.Node.IntegrationTests
{
    public class DebugHyperionSerializer : HyperionSerializer
    {
        public DebugHyperionSerializer(ExtendedActorSystem system) : base(system)
        {
        }

        public DebugHyperionSerializer(ExtendedActorSystem system, Config config) : base(system, config)
        {
        }

        public DebugHyperionSerializer(ExtendedActorSystem system, HyperionSerializerSettings settings) : base(system, settings)
        {
        }

        public override byte[] ToBinary(object obj)
        {
            try
            {
                return base.ToBinary(obj);
            }
            catch(Exception ex)
            {
                int a = 1;
                throw;
            }
        }

        public override object FromBinary(byte[] bytes, Type type)
        {
            try
            {
                return base.FromBinary(bytes, type);
            }
            catch (Exception ex)
            {
                int b = 2;
                throw;
            }
        }
    }
}