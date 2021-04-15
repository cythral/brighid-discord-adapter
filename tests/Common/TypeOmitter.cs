using System.Reflection;

using AutoFixture.Kernel;

internal class TypeOmitter<TType> : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        var propInfo = request as PropertyInfo;
        return propInfo?.PropertyType == typeof(TType) ? new OmitSpecimen() : new NoSpecimen();
    }
}
