using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KSTool.Om.Core;

public static class YamlConverter
{
    public static T DeserializeSettings<T>(string content)
    {
        var builder = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .IgnoreFields();
        ConfigDeserializeBuilder(builder);
        var list = ConfigTagMapping();
        if (list != null) InnerConfigTagMapping(list, builder);

        var ymlDeserializer = builder.Build();

        return ymlDeserializer.Deserialize<T>(content)!;
    }

    public static string SerializeSettings(object obj)
    {
        var builder = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .DisableAliases()
            .IgnoreFields();
        ConfigSerializeBuilder(builder);
        var list = ConfigTagMapping();
        if (list != null) InnerConfigTagMapping(list, builder);
        var converter = builder.Build();
        var content = converter.Serialize(obj);
        return content;
    }

    private static void ConfigSerializeBuilder(SerializerBuilder builder)
    {
        //builder.WithTypeConverter(new DateTimeOffsetConverter());
    }

    private static void ConfigDeserializeBuilder(DeserializerBuilder builder)
    {
        //builder.WithTypeConverter(new DateTimeOffsetConverter());
    }

    private static List<Type>? ConfigTagMapping()
    {
        return null;
    }

    private static void InnerConfigTagMapping<TBuilder>(IEnumerable<Type> list, BuilderSkeleton<TBuilder> builder)
        where TBuilder : BuilderSkeleton<TBuilder>
    {
        foreach (var type in list)
        {
            var convert = PascalCaseNamingConvention.Instance.Apply(GetStandardGenericName(type));
            var url = "tag:yaml.org,2002:" + convert;
            builder.WithTagMapping(url, type);
        }
    }

    private static string GetStandardGenericName(Type type)
    {
        // demo: System.Collection.Generic.List`1[System.String] => System.Collection.Generic.List<System.String>

        if (!type.IsGenericType) return type.FullName!;

        var genericType = type.GetGenericTypeDefinition();
        string? fullName = genericType.FullName;

        var sb = new StringBuilder();

        var index = fullName?.IndexOf('`');
        if (index >= 0)
        {
            sb.Append(fullName![..index.Value]);
        }
        else
        {
            sb.Append(fullName);
        }

        sb.Append('(');
        var args = type.GetGenericArguments();

        for (var i = 0; i < args.Length; i++)
        {
            var innerType = args[i];
            sb.Append(GetStandardGenericName(innerType));
            if (i != args.Length - 1)
            {
                sb.Append(',');
            }
        }

        sb.Append(')');
        return sb.ToString();
    }
}