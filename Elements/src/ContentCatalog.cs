using System.IO;
using System.Linq;
using Elements.Serialization.JSON;
using Newtonsoft.Json.Linq;

namespace Elements
{
    public partial class ContentCatalog
    {
        /// <summary>
        /// Convert the ContentCatalog into it's JSON representation.
        /// </summary>
        public string ToJson()
        {
            JsonInheritanceConverter.ElementwiseSerialization = true;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            JsonInheritanceConverter.ElementwiseSerialization = false;
            return json;
        }

        /// <summary>
        /// Deserialize the give JSON text into the ContentCatalog
        /// </summary>
        /// <param name="json"></param>
        public static ContentCatalog FromJson(string json)
        {
            var catalogObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
            if (catalogObject.ContainsKey("discriminator"))
            {
                return catalogObject.ToObject<ContentCatalog>();
            }
            else if (catalogObject.ContainsKey("Elements") && catalogObject.ContainsKey("Transform")) // catalog is stored in a model
            {
                var model = Model.FromJson(json);
                return model.AllElementsOfType<ContentCatalog>().First();
            }

            return null;
        }

        /// <summary>
        /// Modifies the transforms of the content internal to this catalog to use
        /// the orientation of the reference instances that exist.
        /// </summary>
        public void UseReferenceOrientation()
        {
            if (ReferenceConfiguration == null)
            {
                return;
            }

            foreach (var content in Content)
            {
                var refInstance = ReferenceConfiguration.FirstOrDefault(r => ((ElementInstance)r).BaseDefinition.Id == content.Id) as ElementInstance;
                if (refInstance == null)
                {
                    continue;
                }
                // Use reference instance to set the rotation, but not the position of the original elements.
                var referenceOrientation = refInstance.Transform.Concatenated(new Geometry.Transform(refInstance.Transform.Origin.Negate()));
                content.Transform = referenceOrientation;
            }
        }
    }
}