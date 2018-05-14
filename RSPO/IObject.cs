using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    public interface IObject
    {
        string Name { get; set; }
        ILocation Location { get; set; }
        string Address { get; set; }
        float Price { get; set; } // FIXME: Use as Currency type.
        CurrencyEnum CurrencyType { get; set; }
        float Area { get; set; }
        AreaUnits AreaUnit { get; set; }
        string ImageURL { get; set; }
        string URL { get; set; }
        int Rooms { get; set; }
        int RoomsOffered { get; set; }
        int Floor { get; set; }
        int FloorTotal { get; set; }
        int TotalFloors { get; set; }
        BuildingEnum BuildingType { get; set; }
        IBuildingSeries BuildingSeries { get; set; }
        PropertyEnum PropertyType { get; set; }
        CategoryEnum Category { get; set; }
        string Description { get; set; }
        ICollection<IAgent> Agents { get; set; }
        ICollection<IProperty> Properties { get; set; }
        string GUID { get; set; } // Global ID
    }

    [Entity]
    public interface IOffer
    {
        IObject Object { get; set; }
        OfferEnum OfferType { get; set; }
        string SiteId { get; set; }
        ISite Site { get; set; } // FIXME: Can be null if it is local data
        // FIXME: Creation and update time
        DateTime Created { get; set; }
        DateTime Updated { get; set; }
        IAgent Agent { get; set; }
    }

    [Entity]
    public interface ILocation
    {
        IRegion Region { get; set; }
        string LocalityName { get; set; }
        string SubLocalityName { get; set; }

        [Ignore]
        ICountry Country { get; } // As Region defines the Country it is in.
    }

    public partial class Location : BrightstarEntityObject, ILocation
    {
        public ICountry Country {
            get
            {
                return Region.Country;
            }
        }
    }

    [Entity]
    public interface ICountry
    {
        string Name { get; set; }
    }

    [Entity]
    public interface IRegion
    {
        ICountry Country { get; set; }
        string Name { get; set; }
    }

    [Entity]
    public interface ISite
    {
        string Name { get; set; }
        string URL { get; set; }
    }

    [Entity]
    public interface IBuildingSeries
    {
        string Name { get; set; }
    }

    public enum OfferEnum
    {
        Sale,      // Продажа
        Purchase,  // Покупка
        Rent       // Аренда
    }

    public enum CurrencyEnum
    {
        None,
        RUR,
        USD,
        EUR
        // FIXME: Add more ...
    }

    public enum AreaUnits
    {
        SquaredMeters
        // FIXME: Add more ...
    }

    public enum BuildingEnum
    {
        Unknown,
        Brick,
        BrickMonolyth,
        Monolythn,
        ConcreteBolcks,
        Panel,
        FoamConcrete,
        Wooden,// Деревянный
        Cabin, // Бревенчатый
        Bar,   // Брусовый
        CinderBlock
        // FIXME: Add more ...
    }

    public enum PropertyEnum
    {
        Residental,
        NonResidental,
        Commerсial
    }

    public enum CategoryEnum
    {
        Room,
        Flat,
        House,
        Area,
        Garage,
        Trading,
        Office,
        NonResidental
    }

    public partial class MyEntityContext : BrightstarEntityContext
    {
        public IEntitySet<IBuildingSeries> BuildingSeries
        {
            get
            {
                return BuildingSeriess;
            }
        }
    }

    public partial class Site : BrightstarEntityObject, ISite
    {
    }
}
