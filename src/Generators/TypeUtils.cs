#pragma warning disable SA1204, SA1009, CS1591, SA1600, SA1200, SA1633, IDE0072
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Brighid.Discord.Generators
{
    public static class TypeUtils
    {
        public static object? Cast(object obj, Type type)
        {
            var objParam = Expression.Parameter(typeof(object), "obj");
            var body = Expression.Block(Expression.Convert(Expression.Convert(objParam, obj.GetType()), type));
            var runner = Expression.Lambda(body, objParam).Compile();
            return runner.DynamicInvoke(obj);
        }

        public static T CreateAttribute<T>(AttributeData attributeData)
        {
            var parameters = from arg in attributeData.ConstructorArguments
                             where arg.Type != null
                             let type = LoadType(arg!.Type)
                             select arg.Kind switch
                             {
                                 TypedConstantKind.Type => LoadType(arg.Value as INamedTypeSymbol),
                                 _ => type != null ? Cast(arg.Value!, type) : arg.Value,
                             };

            var result = (T?)Activator.CreateInstance(typeof(T), parameters.ToArray()) ?? throw new Exception();

            foreach (var (key, constant) in attributeData.NamedArguments)
            {
                var value = constant.Value;
                var prop = typeof(T).GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
                var setter = prop?.GetSetMethod(true);
                setter?.Invoke(result, new[] { value });
            }

            return result;
        }

        public static Type? LoadType(ITypeSymbol? symbol)
        {
            return symbol == null
                ? null
                : Assembly.Load(symbol.ContainingAssembly.Name).GetType(symbol.ToString()!);
        }

        public static bool IsSymbolEqualToType(INamedTypeSymbol? symbol, Type? type)
        {
            if (symbol == null || type == null)
            {
                return false;
            }

            var assembly = symbol.ContainingAssembly;
            var typeAssemblyInfo = type.Assembly.GetName();
            var format = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
            var name = symbol.ToDisplayString(format);
            var assemblyName = assembly?.MetadataName;
            var version = assembly?.Identity.Version;
            var publicKeyToken = assembly?.Identity.PublicKeyToken;

#pragma warning disable IDE0078
            if (assembly?.MetadataName == "System.Runtime" || assembly?.MetadataName == "System.Private.CoreLib")
            {
                var runtimeAssemblyInfo = typeof(object).Assembly.GetName();
                assemblyName = runtimeAssemblyInfo.Name;
                version = runtimeAssemblyInfo.Version;
                publicKeyToken = runtimeAssemblyInfo?.GetPublicKeyToken()?.ToImmutableArray();
            }
#pragma warning restore IDE0078

            return
                name == type.FullName &&
                assemblyName == typeAssemblyInfo.Name &&
                version == typeAssemblyInfo.Version &&
                (publicKeyToken?.SequenceEqual(typeAssemblyInfo.GetPublicKeyToken()!) ?? typeAssemblyInfo == null);
        }
    }
}
