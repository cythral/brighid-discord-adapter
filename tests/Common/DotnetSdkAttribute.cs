using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
internal class DotnetSdkAttribute : Attribute
{
    public DotnetSdkAttribute(string version)
    {
        Version = version;
    }

    public string Version { get; }
}
