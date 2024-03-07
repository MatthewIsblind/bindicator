using AngleSharp.Dom;
using AngleSharp.Io;
using AngleSharp.Text;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Text;

public class InputProcessor
{
    public static string ProcessSuburb()
    {
        Console.Write("Enter your suburb: ");
        string suburb = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(suburb))
        {
            suburb = suburb.Trim();
            suburb = char.ToUpper(suburb[0]) + suburb.Substring(1);

            Console.WriteLine("Suburb is: " + suburb);
            return suburb;
        }
        else
        {
            Console.WriteLine("Suburb cannot be empty or whitespace.");
            return "error !!";
        }
    }

    public static string ProcessStreet() 
    {
        Console.Write("Enter your street name (road is rd, street is st, Avenue is ave: ");
        string street = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(street))
        {
            street = street.Trim();
            CultureInfo cultureInfo = new CultureInfo("en-US");
            TextInfo textInfo = cultureInfo.TextInfo;
            street = textInfo.ToTitleCase(street);



            return street;
        }
        else
        {
            Console.WriteLine("street cannot be empty or whitespace.");
            return "error !!";
        }
    }

    public static string ProcessProperty(string streetName)
    {
        Console.Write("Enter your street/unit number ");
        string number = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(number))
        {


            string address = number.Trim() + " " + streetName;

            return address;
        }
        else
        {
            Console.WriteLine("street cannot be empty or whitespace.");
            return "error !!";
        }
    }
}

public class Locality
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Postcode { get; set; }
    public string Council { get; set; }
}


public class LocalityResponse
{
    public List<Locality> Localities { get; set; }
}

//"id":15874,"name":"Alban St","locality":"Oxley"
public class Street 
{ 
    public int Id { get; set; }
    public string Name { get; set; }
    public string Localities { get; set; }
}

public class StreetResponse
{
    public List<Street> Streets { get; set; }
}

class Program
{
    static async Task Main()
    {
        String suburbInput = InputProcessor.ProcessSuburb();
        if (suburbInput != "error !!")
        {


            //find locality id
            string apiUrl = "https://brisbane.waste-info.com.au/api/v1";
            List<Locality> localities = await GetLocalities(apiUrl);

            string localityID = GetLocalityId(localities, suburbInput);
            
            
            //find the street id
            string streetApiUrl = apiUrl + "/streets.json?locality=" + localityID;


            
            Console.WriteLine(streetApiUrl);
            string streetInput = InputProcessor.ProcessStreet();
            Console.WriteLine("street is " + streetInput);
            List<Street> streets = await GetStreets(streetApiUrl);
            string streetID = GetStreetId(streets, streetInput);
            Console.WriteLine("street id is  " + streetID);

            //find the property id (specific address with house/unit number)
            string address = InputProcessor.ProcessProperty(streetInput);
            Console.WriteLine("full address is " + address);
            //string propertyId = GetPropertyId(address);

        }

        static async Task<String> GetPropertyId(string address) {
            return "error";
        }

        static async Task<List<Street>> GetStreets(string apiUrl) {

            string suburbJson = await MakeStreetApiRequest(apiUrl);
            StreetResponse response = JsonConvert.DeserializeObject<StreetResponse>(suburbJson); ;
            if (response != null) {
                return response.Streets;
            }
            return null;
            
        }


        static string GetStreetId(List<Street> streets, string streetInput)
        {
            foreach (Street street in streets)
            {
                if (street.Name.Equals(streetInput))
                {
                    return street.Id.ToString();
                }
            }
            return string.Empty;
        }

        static async Task<String> MakeStreetApiRequest(String  apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();


                        return result;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return "error";
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return "error";
                }
            }
            
        }

        static async Task<List<Locality>> GetLocalities(string apiUrl)
        {
            string endpoint = "/localities.json";
            string suburbJson = await MakeSuburbApiRequest(apiUrl + endpoint);
            LocalityResponse response = JsonConvert.DeserializeObject<LocalityResponse>(suburbJson);

            if (response != null)
            {

                return response.Localities;
            }

            return null;
        }


        static string GetLocalityId(List<Locality> localities, string targetSuburb)
        {
            foreach (Locality locality in localities)
            {
                if (locality.Name.Equals(targetSuburb))
                {
                    return locality.Id.ToString();
                }
            }
            return string.Empty; // Return an appropriate value or handle the case where the locality is not found
        }






        static async Task<string> MakeSuburbApiRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        
                        
                        return result;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return "error";
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return "error";
                }
            }
        }


        




    }

    
}
    
