﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SharpCompress.Readers;

namespace RSPO
{
    public class ImportFromAtlcomru
    {
        public string FileName { get; set; } = null;
        public Stream InputStream { get; set; } = null;
        public XDocument Document {
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

                ISite site = Application.Context.Sites.Create(); // FIXME: Must Be Query/Create

                site.Name =  "Атлант-Недвижимость";
                site.URL  = @"http://atlantnt.ru";

                foreach (XElement p in proposals)
                {
                    ProcessProposal(p, site);
                }
            }
        }

        private void dbg(string msg)
        {
            Console.WriteLine("---> "+msg);
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

            obj.PropertyType = GetPropertyType(input);
            obj.Category = GetCategoryType(input);
            obj.URL = GetText(input, "url");
            // FIXME: Accept Dates into offer
            // FIXME: ManuallyAdded

            XElement location = GetFirstElement(input, "location");

            ctx.Add(obj);
            ctx.Add(offer);

            ctx.SaveChanges();
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
            {"пенобетонный"       , BuildingEnum.FoamConcrete}
        };

        protected BuildingEnum GetBuildingType(XElement i, string tagName="building-type")
        {
            return buildingTypes[GetText(i, tagName)];
        }

        private Dictionary<string,PropertyEnum> propertyTypes = new Dictionary<string,PropertyEnum>
        {
            {"жилая"      , PropertyEnum.Living},
        };

        protected PropertyEnum GetPropertyType(XElement i, string tagName="property-type")
        {
            return propertyTypes[GetText(i, tagName)];
        }

        private Dictionary<string,CategoryEnum> categoryTypes = new Dictionary<string,CategoryEnum>
        {
            {"Комната" , CategoryEnum.Room},
            {"Квартира", CategoryEnum.Flat},
            {"Дом"     , CategoryEnum.House}
        };

        protected CategoryEnum GetCategoryType(XElement i, string tagName="category")
        {
            return categoryTypes[GetText(i, tagName)];
        }



        // Auxiliary methods working with XML tree

        private string GetText(XElement e, string tagName)
        {
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
