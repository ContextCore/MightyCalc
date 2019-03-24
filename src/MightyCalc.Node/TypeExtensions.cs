using System;
using System.Reflection;

namespace MightyCalc.Node
{
    public static class TypeExtensions
    {
        public static string AssemblyQualifiedShortName(this Type type)
        {
            return type.FullName + ", " + type.GetTypeInfo().Assembly.GetName().Name;
        }
    }
}