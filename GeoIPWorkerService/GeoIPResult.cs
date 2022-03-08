using MaxMind.GeoIP2.Model;
using MaxMind.GeoIP2.Responses;
using System;

namespace GeoIPWorkerService
{
    public class GeoIPResult
    {
        public GeoIPResult(CityResponse response)
        {
            Country = response.Country;
            MostSpecificSubdivision = response.MostSpecificSubdivision;
            City = response.City;
            Postal = response.Postal;
            Location = response.Location;
        }

        private Country Country { get; set; }

        private Subdivision MostSpecificSubdivision { get; set; }

        private City City { get; set; }

        private Postal Postal { get; set; }

        private Location Location { get; set; }

        public Guid CorrelationId { get; set; }

        public string? IsoCode 
        { 
            get { return Country.IsoCode; }
        }

        public string? CountryName 
        { 
            get { return Country.Name; }
        }

        public string? MostSpecificSubdivisionName
        {
            get { return MostSpecificSubdivision.Name; }
        }

        public string? MostSpecificSubdivisionIsoCode
        {
            get { return MostSpecificSubdivision.IsoCode; }
        }

        public string? CityName
        {
            get { return City.Name; }
        }

        public string? PostalCode
        {
            get { return Postal.Code; }
        }

        public double? Latitude
        {
            get { return Location.Latitude; }
        }

        public double? Longitude
        {
            get { return Location.Longitude; }
        }
    }
}
