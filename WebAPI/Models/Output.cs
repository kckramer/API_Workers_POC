using System;
using System.Linq;

namespace WebAPI.Models
{
    public class Output
    {
        public Guid CorrelationId { get; set; }

        public string? IsoCode {get; set;}

        public string? CountryName { get; set; }

        public string? MostSpecificSubdivisionName { get; set; }

        public string? MostSpecificSubdivisionIsoCode { get; set; }

        public string? CityName { get; set; }

        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? DomainDetails { get; set; }

        public static void CopyValues<T>(T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
        }
    }
}