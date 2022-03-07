namespace WebAPI.Models
{
    public class Output
    {
        public string? IsoCode {get; set;}

        public string? CountryName { get; set; }

        public string? MostSpecificSubdivisionName { get; set; }

        public string? MostSpecificSubdivisionIsoCode { get; set; }

        public string? CityName { get; set; }

        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? DomainDetails { get; set; }
    }
}