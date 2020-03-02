using adenin.IntentRecognizer.Store.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;

namespace AzureSearchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SearchIndexClient indexClient = new SearchIndexClient("adenin-intents", "utterances", new SearchCredentials("INDEX_QUERY_KEY"));

            SearchParameters parameters = new SearchParameters
            {
                SearchMode = SearchMode.All,
                QueryType = QueryType.Full
            };

            string searchText = "id:1=PTO-STATUS=my_PTO";

            Console.WriteLine($"Executing query 'searchMode={parameters.SearchMode}&queryType={parameters.QueryType}&search={searchText}'\n");

            // Repeat the attempt 5 times
            for (int i = 0; i < 5; i++)
            {
                DocumentSearchResult<Utterance> searchResults = indexClient.Documents.Search<Utterance>(searchText, parameters);

                Console.WriteLine($"Attempt {i + 1} returned {searchResults.Results.Count} results:");

                foreach (SearchResult<Utterance> result in searchResults.Results)
                {
                    Console.WriteLine($"Utterance id: {result.Document.Id}");
                }

                Console.WriteLine("\n");
            }
        }
    }
}
