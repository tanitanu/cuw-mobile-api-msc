using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CCE
{
    public class SDLContentResponseData
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public SDLBody[] Body { get; set; }
        public SDLResult[] Results { get; set; }
        public object[] ErrorList { get; set; }
    }

    public class SDLBody
    {
        public SDLPresentation presentation { get; set; }
        public SDLContent[] content { get; set; }
        public SDLMetadata metadata { get; set; }
        public SDLExtendedproperties extendedProperties { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Body { get; set; }
        public object Results { get; set; }
        public object Page { get; set; }
        public object PageLayout { get; set; }
        public object PageMetadata { get; set; }
        public object PageData { get; set; }
        public object Navigation { get; set; }
        public object ComponentPresentations { get; set; }
        public SDLErrorlist[] ErrorList { get; set; }
        public string LastCallDateTime { get; set; }
        public string CallTime { get; set; }
        public object SessionId { get; set; }
    }

    public class SDLPresentation
    {
        public string view { get; set; }
        public string designName { get; set; }
        public string designId { get; set; }
    }

    public class SDLMetadata
    {
        public string nav_title { get; set; }
        public string show_sitemap { get; set; }
        public string show_in_breadcrumb { get; set; }
        public string include_in_atw { get; set; }
    }

    public class SDLExtendedproperties
    {
        public string fileName { get; set; }
    }

    public class SDLContent
    {
        public SDLPresentation1 presentation { get; set; }
        public SDLContent1 content { get; set; }
        public SDLMetadata1 metadata { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class SDLPresentation1
    {
        public string view { get; set; }
        public string header_tag { get; set; }
        public string header_tag_increment { get; set; }
        public string designId { get; set; }
        public string designName { get; set; }
    }

    public class SDLContent1
    {
        public string key { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string body_text { get; set; }
        public string subtitle { get; set; }

        public SDLSection[] sections { get; set; }
        public string src { get; set; }
        public SDLImageSource image_source { get; set; }
        public List<SDLImageAsset> image_assets { get; set; }
    }

    public class SDLImageAsset
    {
        public SDLContent1 content { get; set; }

    }

    public class SDLImageSource
    {
        public SDLContent1 content { get; set; }
    }

    public class SDLContent3
    {
        public SDLImageSource image_source { get; set; }
    }


    public class SDLSection
    {
        public SDLContent2 content { get; set; }
        public SDLMetadata1 metadata { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class SDLContent2
    {
        public string key { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }

    public class SDLMetadata1
    {
        public string contentModel { get; set; }
    }


    public class SDLErrorlist
    {
        public int Code { get; set; }
        public object Message { get; set; }
        public object FieldName { get; set; }
    }

    public class SDLResult
    {
        public string id { get; set; }
        public object Description { get; set; }
        public object Category { get; set; }
        public object GroupName { get; set; }
        public object Image { get; set; }
        public object ItemKey { get; set; }
        public string ComponentTitle { get; set; }
        public string ComponentSchema { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public SDLProduct[] Products { get; set; }
    }

    public class SDLProduct
    {
        public string id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ComponentTitle { get; set; }
        public string ComponentSchema { get; set; }
        public string Id { get; set; }
        public object View { get; set; }
        public object Controller { get; set; }
        public object Action { get; set; }
        public object Region { get; set; }
        public object BlockSize { get; set; }
        public string HeaderTag { get; set; }
        public bool IncrementHeaderTag { get; set; }
        public object FormAction { get; set; }
        public string ActionEndpoint { get; set; }
        public object ItemWeight { get; set; }
        public object Location { get; set; }
        public string OfferTile { get; set; }
        public string ConfigDetails { get; set; }
    }
}
