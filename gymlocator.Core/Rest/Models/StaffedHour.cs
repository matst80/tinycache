// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace gymlocator.Rest.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// A time span with an optional label. From and To should always be the
    /// same date. Unless it is a deviation, only the weekday part of the date
    /// is of interest. If this is an exception date, the actual date is also
    /// interesting.
    /// </summary>
    public partial class StaffedHour
    {
        /// <summary>
        /// Initializes a new instance of the StaffedHour class.
        /// </summary>
        public StaffedHour()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the StaffedHour class.
        /// </summary>
        /// <param name="fromProperty">Start time/date of staffed hour. The
        /// format is ISO-8601.</param>
        /// <param name="to">End time/date of staffed hour. Date will always be
        /// the same date as `from`. The format is ISO-8601.</param>
        /// <param name="deviation">This is a string for a deviation to the
        /// regular schedule. If this value is null, the record is part of the
        /// standard schedule.</param>
        public StaffedHour(System.DateTime fromProperty, System.DateTime to, string deviation = default(string))
        {
            FromProperty = fromProperty;
            To = to;
            Deviation = deviation;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets start time/date of staffed hour. The format is
        /// ISO-8601.
        /// </summary>
        [JsonProperty(PropertyName = "from")]
        public System.DateTime FromProperty { get; set; }

        /// <summary>
        /// Gets or sets end time/date of staffed hour. Date will always be the
        /// same date as `from`. The format is ISO-8601.
        /// </summary>
        [JsonProperty(PropertyName = "to")]
        public System.DateTime To { get; set; }

        /// <summary>
        /// Gets or sets this is a string for a deviation to the regular
        /// schedule. If this value is null, the record is part of the standard
        /// schedule.
        /// </summary>
        [JsonProperty(PropertyName = "deviation")]
        public string Deviation { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            //Nothing to validate
        }
    }
}
