using System.Reflection;

internal static class TypeUtils
{
    public static void SetPrivateField(this object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }

    public static void SetPrivateProperty(this object target, string propertyName, object value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        var setter = property?.GetSetMethod(true);
        setter?.Invoke(target, new[] { value });
    }
}
