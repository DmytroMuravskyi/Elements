//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v10.1.21.0 (Newtonsoft.Json v12.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------
using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Properties;
using Elements.Validators;
using Elements.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Elements.Geometry.Line;
using Polygon = Elements.Geometry.Polygon;

namespace Elements
{
    #pragma warning disable // Disable all warnings

    /// <summary>A collection of ContentElements</summary>
    [Newtonsoft.Json.JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v12.0.0.0)")]
    [UserElement]
	public partial class ContentCatalog : Element
    {
        [Newtonsoft.Json.JsonConstructor]
        public ContentCatalog(IList<ContentElement> @content, System.Guid @id, string @name)
        	: base(id, name)
        {
        	var validator = Validator.Instance.GetFirstValidatorForType<ContentCatalog>();
        	if(validator != null)
        	{
        			validator.PreConstruct(new object[]{ @content, @id, @name});
        	}
        
        		this.Content = @content;
        	
        	if(validator != null)
        	{
        		validator.PostConstruct(this);
        	}
        }
    
        /// <summary>The content elements in this catalog.</summary>
        [Newtonsoft.Json.JsonProperty("Content", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public IList<ContentElement> Content { get; set; } = new List<ContentElement>();
    
    
    }
}