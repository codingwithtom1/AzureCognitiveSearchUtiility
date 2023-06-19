using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AzureCognitiveSearchUtility
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                   .AddJsonFile($"appsettings.json", true, true);
            IConfiguration config = builder.Build();

            var serviceName = config["ServiceName"];
            var indexName = config["IndexName"];
            var apiKey = config["APIKey"];
            var serviceEndpointURL = config["ServiceEndpointURL"];
            

            Uri serviceEndpoint = new Uri(serviceEndpointURL);
            AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            SearchClient client = new SearchClient(serviceEndpoint, indexName, credential);

            // read knowledgebaseentries from file
            string fileName = "knowledgebase.json";
            string jsonString = File.ReadAllText(fileName);
            var knowledgebaseentries =
                JsonSerializer.Deserialize<List<KnowledgeBaseEntry>>(jsonString);

            // Insert the document into the index
            Response<IndexDocumentsResult> idxresult = await client.UploadDocumentsAsync(knowledgebaseentries);


            
            string myquestion = "I just started working 3 months ago, will my routine doctor visit be covered by the insurance?";
            SearchOptions options = new SearchOptions() { Size = 3 };
            SearchResults<KnowledgeBaseEntry> results = await client.SearchAsync<KnowledgeBaseEntry>(myquestion,options);

            Console.WriteLine("Question:");
            Console.WriteLine(myquestion);
            Console.WriteLine("Answers:");
            foreach (SearchResult<KnowledgeBaseEntry> result in results.GetResults())
            {
                Console.WriteLine(result.Document.id+":"+result.Document.Body);
            }

        }
    }
    

   
    public class KnowledgeBaseEntry
    {
        [SimpleField(IsKey = true)]
        public string? id { get; set; }
        [SearchableField]
        public string? Department { get; set; }
        [SearchableField]
        public string? Topic { get; set; }
        [SearchableField]
        public string? Body { get; set; }
        [SearchableField]
        public string? Owner { get; set; }
    }
}