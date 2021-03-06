// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace gymlocator.Rest.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Represents an image with meta data.
    /// </summary>
    public partial class Image
    {
        /// <summary>
        /// Initializes a new instance of the Image class.
        /// </summary>
        public Image()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Image class.
        /// </summary>
        /// <param name="name">Name of the image, filename possibly</param>
        /// <param name="url">Full URL to the image location</param>
        /// <param name="width">Width of the image in pixels.</param>
        /// <param name="height">Height of the image in pixels.</param>
        /// <param name="description">Description of the image, possible use as
        /// alt tag</param>
        public Image(string name, string url, int width, int height, string description = default(string))
        {
            Name = name;
            Description = description;
            Url = url;
            Width = width;
            Height = height;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets name of the image, filename possibly
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the image, possible use as alt tag
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets full URL to the image location
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets width of the image in pixels.
        /// </summary>
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets height of the image in pixels.
        /// </summary>
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Name");
            }
            if (Url == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Url");
            }
        }
    }
}
