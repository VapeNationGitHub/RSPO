using BrightstarDB.EntityFramework;

namespace RSPO
{
    [Entity]
    public interface IProperty
    {
        IPropertyType Type { get; set; }
        string Value { get; set; }
    }
}