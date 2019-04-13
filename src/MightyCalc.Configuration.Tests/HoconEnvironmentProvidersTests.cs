using System;
using Akka.Configuration;
using Xunit;

namespace MightyCalc.Configuration.Tests
{
        public class HoconEnvironmentProvidersTests
        {
            [Theory]
            [InlineData("MightyCalc_TEST_EVN_Quoted", "test.me", "123", "\"123\"")]
            [InlineData("MightyCalc_TEST_EVN_Quoted", "test.me.test", "1 23", "\"1 23\"")]
            [InlineData("MightyCalc_TEST_EVN_Quoted", "test.me.yes", " ", "\"\"")]
            public void Provider_can_extract_quoted_variables(string envName, string path, string value, string expectedValue)
            {
                Environment.SetEnvironmentVariable(envName, value);
                var provider = Configuration.HoconValueProvider.Quoted(envName, path);
                var hocon = provider.GetHocon();
                var extracted = hocon.Replace(path + " = ", "");
                Assert.Equal(expectedValue,extracted);
            }
            
            [Theory]
            [InlineData("MightyCalc_TEST_EVN_Array", "test.me", "1- 2; 3!", "[\"1-\",\"2;\",\"3!\"]")]
            [InlineData("MightyCalc_TEST_EVN_Array", "test.me", "1", "[\"1\"]")]
            [InlineData("MightyCalc_TEST_EVN_Array", "test.me", " ", "[]")]
            public void Provider_can_extract_array_variables(string envName, string path, string value, string expectedValue)
            {
                Environment.SetEnvironmentVariable(envName, value);
                var provider = Configuration.HoconValueProvider.Array(envName, path);
                var hocon = provider.GetHocon();
                var extracted = hocon.Replace(path + " = ", "");
                Assert.Equal(expectedValue,extracted);
            }

            [Fact]
            public void Extract_quoted_for_missing_env_variable_gives_null()
            {
                var provider = Configuration.HoconValueProvider.Quoted("NOT_EXIST", "test.me");
                var hocon = provider.GetHocon();
                Assert.True(string.IsNullOrEmpty(hocon));
            }
            
            [Fact]
            public void Extract_array_for_missing_env_variable_gives_null()
            {
                var provider = Configuration.HoconValueProvider.Array("NOT_EXIST", "test.me");
                var hocon = provider.GetHocon();
                Assert.True(string.IsNullOrEmpty(hocon));
            }
        }
}