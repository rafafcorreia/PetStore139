using System.Data.Common;
using models;
using Newtonsoft.Json;
using RestSharp;

namespace tests;

public class PetTest
{
    private const String BASE_URL = "https://petstore.swagger.io/v2/";

    public static IEnumerable<TestCaseData> getTestData()
    {
        String caminhoMassa = @"C:\testspace\PetStore139\fixtures\pet.csv";

        using var reader = new StreamReader(caminhoMassa);

        // Pula a primeira linha com os cabeçahos
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(", ");

            yield return new TestCaseData(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], values[5], values[6], values[7]);
        }

    }

    private RestClient client;
    [SetUp]
    public void SetUp()
    {
        client = new RestClient(BASE_URL);
    }

    [Test, Order(1)]
    public void PostPetTest()
    {
        // var client = new RestClient(BASE_URL);
        var request = new RestRequest("pet", Method.Post);

        String jsonBody = File.ReadAllText(@"C:\testspace\PetStore139\fixtures\pet1.json");
        request.AddBody(jsonBody);

        var response = client.Execute(request);
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        String status = responseBody.status;
        Assert.That(status, Is.EqualTo("available"));
        int petId = responseBody.id;
        Environment.SetEnvironmentVariable("petId", petId.ToString());
    }

    [Test, Order(2)]
    public void GetPetTest()
    {
        // int petId = 9102023;
        String petName = "Spike";

        // var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{Environment.GetEnvironmentVariable("petId")}", Method.Get);
        var response = client.Execute(request);
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        int petId = Int32.Parse(Environment.GetEnvironmentVariable("petId"));

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petId));
        Assert.That((string)responseBody.name, Is.EqualTo(petName));
        Assert.That((int)responseBody.category.id, Is.EqualTo(0));
        Assert.That((int)responseBody.tags[0].id, Is.EqualTo(0));
        Assert.That((string)responseBody.tags[0].name, Is.EqualTo("string"));
    }

    [Test, Order(3)]
    public void PutPetTest()
    {
        PetModel petModel = new PetModel();
        petModel.id = Int32.Parse(Environment.GetEnvironmentVariable("petId"));
        petModel.category = new Category(0, "string");
        petModel.name = "Spike";
        petModel.photoUrls = new String[] { "string" };
        petModel.tags = new Tag[] { new Tag(0, "string") };
        petModel.status = "unavaiable";
        String jsonString = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonString);

        // var client = new RestClient(BASE_URL);
        var request = new RestRequest("pet", Method.Put);
        request.AddBody(jsonString);

        var response = client.Execute(request);
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(responseBody.status.ToString(), Is.EqualTo("unavaiable"));
    }

    [Test, Order(4)]
    public void DeletePetTest()
    {
        int petId = Int32.Parse(Environment.GetEnvironmentVariable("petId"));

        // var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Delete);
        var response = client.Execute(request);
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        Assert.That(responseBody.message.ToString(), Is.EqualTo(petId.ToString()));

        Console.WriteLine($"Variável de ambiente (petId): {Environment.GetEnvironmentVariable("petId")}");
    }

    [TestCaseSource("getTestData", new object[] { }), Order(5)]
    public void PostPetTestDDT(int petId, int categoryId, String categoryName, String petName, String photoUrls, String tagsIds, String tagsNames, String status)
    {
        // var client = new RestClient(BASE_URL);
        var request = new RestRequest("pet", Method.Post);

        PetModel petModel = new PetModel();
        petModel.id = petId;
        petModel.category = new Category(categoryId, categoryName);
        petModel.name = petName;
        petModel.photoUrls = new String[] { photoUrls };

        String[] tagsIdsList = tagsIds.Split("; ");
        String[] tagsNamesList = tagsNames.Split("; ");
        List<Tag> tagsList = new List<Tag>();

        petModel.tags = new Tag[tagsIds.Length];

        for (int i = 0; i < tagsIdsList.Length; i++)
        {
            int tagId = int.Parse(tagsIdsList[i]);
            String tagName = tagsNamesList[i];

            Tag tag = new Tag(tagId, tagName);
            tagsList.Add(tag);
        }
        petModel.tags = tagsList.ToArray();
        petModel.status = status;
        String jsonString = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonString);
        request.AddBody(jsonString);

        var response = client.Execute(request);
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        String respStatus = responseBody.status;
        Assert.That(respStatus, Is.EqualTo(status));
        // int petId = responseBody.id;
        // Environment.SetEnvironmentVariable("petId", petId.ToString());
    }
}
