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

namespace Elements.Geometry
{
#pragma warning disable // Disable all warnings

    /// <summary>A UV coordinate.</summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v12.0.0.0)")]
    public struct UV
    {
        [Newtonsoft.Json.JsonConstructor]
        public UV(double @u, double @v)
        {
            var validator = Validator.Instance.GetFirstValidatorForType<UV>();
            if (validator != null)
            {
                validator.PreConstruct(new object[] { @u, @v });
            }

            this.U = @u;
            this.V = @v;

            if (validator != null)
            {
                validator.PostConstruct(this);
            }
        }

        /// <summary>The U coordinate.</summary>
        [Newtonsoft.Json.JsonProperty("U", Required = Newtonsoft.Json.Required.Always)]
        public double U { get; set; }

        /// <summary>The V coordinate.</summary>
        [Newtonsoft.Json.JsonProperty("V", Required = Newtonsoft.Json.Required.Always)]
        public double V { get; set; }


}
}