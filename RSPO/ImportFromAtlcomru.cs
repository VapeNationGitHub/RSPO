using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BrightstarDB.EntityFramework;
using SharpCompress.Readers;
using System.IO;

namespace RSPO
{
    public class ImportFromAtlcomru
    {
        private bool DEBUG = false;

        public string FileName { get; set; } = null;
        public Stream InputStream { get; set; } = null;
        public XDocument Document { // Меня тут студентка просит помочь. Надо ее за рога брать, пока не потерялась...
            // Так что перерыв. Хотя она пока тормозит можно параллельно башлять.

            get
            {
                if (_document == null)
                {
                    LoadDocument(); // Must set _document to nonNull value or raise an Exception.
                }
                return _document;
            }
        }

        private void LoadDocument()
        {
            if (FileName == null && InputStream == null)
            {
                throw new RCFileException("no file nor a stream supplied for import");
            }

            if (FileName != null && InputStream != null)
            {
                throw new RCFileException("file and a stream supplied for import");
            }

            if (FileName != null)
            {
                InputStream=File.Open(FileName, FileMode.Open);
            }

            // Check wether InputStream is an archive

            Stream actualStream = null;
            try
            {
                IReader reader = ReaderFactory.Open(InputStream);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        // Console.WriteLine(String.Format("Entry:{0}", reader.Entry));
                        actualStream=reader.OpenEntryStream();
                        break;
                    }
                }
                if (actualStream == null)
                {
                    throw new RCFormatException("cannot find a file in archive");
                }
            }
            catch (InvalidOperationException)
            {
                actualStream=InputStream;
                if (actualStream.CanSeek)
                {
                    actualStream.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    throw new RCOperationException("cannot seek file to reset reading");
                }
            }

            _document = XDocument.Load(actualStream);
            actualStream.Close();
        }

        private XDocument _document = null;

        public void Import(bool onlyLoad=false)
        {
            XDocument doc = Document; // Загружает XML, еще необработанный, дерево.
            MyEntityContext ctx = Application.Context;

            if (! onlyLoad)
            {
                Console.WriteLine("Processing doc!");
                /*
                IEnumerable<XElement> proposals =
                    from p in doc.Elements()
                    select p;
                    */

                IEnumerable<XElement> proposals =
                    doc.Descendants(YName("offer"));


                string siteName = "Атлант-Недвижимость";
                string siteURL = @"http://atlantnt.ru";
                ISite site = ctx.Sites.Where(x=>x.URL.Equals(siteURL)).FirstOrDefault();

                if (site == null) {
                    site=ctx.Sites.Create();
                    site.Name = siteName;
                    site.URL  = siteURL;
                    ctx.SaveChanges();
                }

                int count = proposals.Count(), number = 0;
                foreach (XElement p in proposals)
                {
                    if (number % 100 == 0)
                    {
                        Console.Write(String.Format("Rec {0} of {1}    \r", number, count));
                        Application.Context.SaveChanges();
                    };
                    ProcessProposal(p, site);
                    number++;
                };

                Application.Context.SaveChanges();


            }
        }

        private void dbg(string msg)
        {
            if (DEBUG)
            {
                Console.WriteLine("---> "+msg);
            }
        }

        protected void ProcessProposal(XElement input, ISite site)
        {
            MyEntityContext ctx = Application.Context;
            XAttribute internalId = input.Attribute("internal-id");  // Value

            IOffer offer = ctx.Offers.Create();
            IObject obj = ctx.Objects.Create();

            offer.Object = obj;
            offer.SiteId = internalId.Value;
            offer.OfferType = GetOfferType(input);
            offer.Site = site;

            offer.Created=GetDateTime(input, "creation-date");
            offer.Updated=GetDateTime(input, "last-update-date");

            GetSalesAgent(offer, input);

            // FIXME: The code implies the objects are unique.
            // TODO: Existence check

            obj.PropertyType = GetPropertyType(input);
            obj.Category = GetCategoryType(input);
            obj.URL = GetText(input, "url");
            obj.GUID=GetGUID();

            GetLocationData(obj, input);
            GetPrice(obj, input);
            try {
                obj.ImageURL=GetText(input, "image");
            } catch (InvalidOperationException) {
                obj.ImageURL=null;
            }
            try {
                obj.Rooms=int.Parse(GetText(input, "rooms"));
            } catch (InvalidOperationException) {
                obj.Rooms=0;
            }
            try {
                obj.RoomsOffered=int.Parse(GetText(input, "rooms-offered"));
            } catch (InvalidOperationException) {
                obj.RoomsOffered=0;
            }
            try {
                obj.Floor=int.Parse(GetText(input, "floor"));
            } catch (InvalidOperationException) {
                obj.Floor=-1000;
            }

            try {
                obj.FloorTotal=int.Parse(GetText(input, "floors-total"));
            } catch (InvalidOperationException) {
                obj.FloorTotal=-1000;
            }

            try {
                obj.BuildingType=GetBuildingType(input);
            } catch (InvalidOperationException) {
                obj.BuildingType=BuildingEnum.Unknown;
            };
            try {
                obj.BuildingSeries=GetBuildingSeries(input);
            } catch (InvalidOperationException) {
                obj.BuildingSeries = null;
            }
            obj.Description=GetText(input, "description");

            ctx.Add(obj);
            ctx.Add(offer);

        }

        protected void GetSalesAgent(IOffer offer, XElement input, string tagName="sales-agent")
        {

            XElement sa = GetFirstElement(input, tagName);

            RoleEnum saRole = GetRole(sa);
            string name = GetText(sa, "name");
            string phone = GetText(sa, "phone");

            MyEntityContext ctx = Application.Context;
            IAgent agent = ctx.Agents.Where(x=>x.Name==name &&
                                            x.Phone==phone).FirstOrDefault();
            if (agent == null)
            {
                agent = ctx.Agents.Create();
                agent.GUID = GetGUID();
                agent.Name = name;
                agent.Phone = phone;
                agent.Role = saRole;
                ctx.Add(agent);
                ctx.SaveChanges();
            }
            offer.Agent = agent;

            // FIXME: Implement
        }

        protected void GetPrice(IObject obj, XElement input, string tagName="price")
        {
            obj.Price=float.Parse(GetText(input, "value"));
            obj.CurrencyType = GetCurrencyType(input);
        }

        protected DateTime GetDateTime(XElement input, string tagName)
        {
            return DateTime.Parse(GetText(input, tagName));
        }

        protected void GetLocationData(IObject obj, XElement input, string tagName="location")
        {
            MyEntityContext ctx=Application.Context;
            XElement locInput = GetFirstElement(input, tagName);

            string coName = GetText(locInput, "country");
            ICountry co=ctx.Countries.Where(x=>x.Name.Equals(coName)).FirstOrDefault();
            if (co==null)
            {
                co=ctx.Countries.Create();
                co.Name=coName;
                ctx.Add(co);
                ctx.SaveChanges();
            }

            string regName = GetText(locInput, "region");
            IRegion reg = ctx.Regions.Where(x => x.Name==regName &&
                                            x.Country.Name==coName).FirstOrDefault();
            if (reg==null)
            {
                reg=ctx.Regions.Create();
                reg.Country = co;
                reg.Name = regName;
                ctx.SaveChanges();
            }

            string locLN = GetText(locInput, "locality-name");
            string locSLN = GetText(locInput, "sub-locality-name");

            ILocation loc=ctx.Locations.Where(x=>
                                              x.Region.Country.Name==coName &&
                                              x.Region.Name==regName &&
                                              x.LocalityName==locLN &&
                                              x.SubLocalityName==locSLN).FirstOrDefault();
            if (loc == null) {
                loc = ctx.Locations.Create();
                loc.Region = reg;
                loc.LocalityName = locLN;
                loc.SubLocalityName = locSLN;
            }
            obj.Location = loc;
            try {
                obj.Address = GetText(locInput, "address");
            } catch (InvalidOperationException) {
                obj.Address = null;
            }
        }

        private Dictionary<string,OfferEnum> offerTypes = new Dictionary<string,OfferEnum>
        {
            {"продажа", OfferEnum.Sale},
            {"аренда" , OfferEnum.Rent},
            {"покупка", OfferEnum.Purchase}
        };

        protected OfferEnum GetOfferType(XElement i, string tagName="type")
        {
            return offerTypes[GetText(i, tagName)];
        }

        private Dictionary<string,AreaUnits> areaUnits = new Dictionary<string,AreaUnits>
        {
            {"кв.м", AreaUnits.SquaredMeters}
        };

        protected AreaUnits GetAreaUnit(XElement i, string tagName="area")
        {
            return areaUnits[GetText(i, tagName)];
        }


        private Dictionary<string,BuildingEnum> buildingTypes = new Dictionary<string,BuildingEnum>
        {
            {"кирпичный"          , BuildingEnum.Brick},
            {"кирпично-монолитный", BuildingEnum.BrickMonolyth},
            {"монолитный"         , BuildingEnum.Monolythn},
            {"панельный"          , BuildingEnum.Panel},
            {"пенобетонный"       , BuildingEnum.FoamConcrete},
            {"деревянный"         , BuildingEnum.Wooden},
            {"бревенчатый"        , BuildingEnum.Cabin},
            {"бетоноблочный"      , BuildingEnum.ConcreteBolcks},
            {"шлакоблочный"       , BuildingEnum.CinderBlock},
            {"брусовый"           , BuildingEnum.CinderBlock},
        };

        protected BuildingEnum GetBuildingType(XElement i, string tagName="building-type")
        {
            return buildingTypes[GetText(i, tagName)];
        }

        private Dictionary<string,PropertyEnum> propertyTypes = new Dictionary<string,PropertyEnum>
        {
            {"жилая"   , PropertyEnum.Residental},
            {"нежилая" , PropertyEnum.NonResidental},
            {"коммерческая" , PropertyEnum.Commerсial},
        };

        protected PropertyEnum GetPropertyType(XElement i, string tagName="property-type")
        {
            return propertyTypes[GetText(i, tagName)];
        }

        private Dictionary<string,CategoryEnum> categoryTypes = new Dictionary<string,CategoryEnum>
        {
            {"Комната" , CategoryEnum.Room},
            {"Квартира", CategoryEnum.Flat},
            {"Участок" , CategoryEnum.Area},
            {"Нежилое" , CategoryEnum.NonResidental},
            {"Торговое", CategoryEnum.Trading},
            {"Гараж"   , CategoryEnum.Garage},
            {"Офис"    , CategoryEnum.Office},
            {"Дом"     , CategoryEnum.House}
        };

        protected CategoryEnum GetCategoryType(XElement i, string tagName="category")
        {
            return categoryTypes[GetText(i, tagName)];
        }

        private Dictionary<string, RoleEnum> roles = new Dictionary<string, RoleEnum>
        {
            {"владелец" , RoleEnum.Owner},
            {"агентство" , RoleEnum.Agent},
        };

        // private HashSet<string> uKeys = new HashSet<string>();

        protected RoleEnum GetRole(XElement i, string tagName="category")
        {
            return roles[GetText(i, tagName)];
        }

        protected IBuildingSeries GetBuildingSeries(XElement i, string tagName = "building-series")
        {
            MyEntityContext ctx = Application.Context;
            string seriesName = GetText(i, tagName);
            IBuildingSeries s = ctx.BuildingSeries.Where(x=>x.Name.Equals(seriesName)).FirstOrDefault();
            if (s==null)
            {
                s = ctx.BuildingSeries.Create();
                s.Name = seriesName;
            }
            return s;
        }

        private Dictionary<string, CurrencyEnum> currens = new Dictionary<string, CurrencyEnum>
        {
            {"RUR" , CurrencyEnum.RUR},
            {"USD" , CurrencyEnum.USD},
            {"EUR" , CurrencyEnum.EUR},
        };

        // private HashSet<string> uKeys = new HashSet<string>();

        protected CurrencyEnum GetCurrencyType(XElement i, string tagName="currency")
        {
            return currens[GetText(i, tagName)];
        }


        // Auxiliary methods working with XML tree

        private string GetText(XElement e, string tagName)
        {
            dbg("GetText: "+tagName);
            return GetFirstElement(e, tagName).Value;
        }

        private XElement GetFirstElement(XElement e, string tagName)
        {
            return e.Descendants(YName(tagName)).First();
        }

        protected XName YName(string name)
        {
            return XName.Get(name, YandexNS);
        }

        protected readonly string YandexNS = @"http://webmaster.yandex.ru/schemas/feed/realty/2010-06";

        public string GetGUID()
        {
            return Guid.NewGuid().ToString();
        }

    }

    // Exceptions

    public class RCException: Exception
    {
        public RCException(string message) : base(message)
        {
        }
    }

    public class RCFileException: RCException {
        public RCFileException(string message): base(message)
        {
        }
    }

    public class RCFormatException: RCException
    {
        public RCFormatException(string message) : base(message)
        {
        }
    }

    public class RCOperationException: RCException
    {
        public RCOperationException(string message) : base(message)
        {
        }
    }
}
