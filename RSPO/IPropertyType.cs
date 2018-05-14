using BrightstarDB.EntityFramework;
using System.Collections.Generic;

namespace RSPO
{
    [Entity]
    public interface IPropertyType
    {
        string Name { get; set; }
        float Significance { get; set; }
        [InverseProperty("Type")]
        ICollection <IProperty> Properties { get; set; }
    }
}