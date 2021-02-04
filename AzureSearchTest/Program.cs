using adenin.IntentRecognizer.Store.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSearchTest
{
    class Program
    {
        async static Task Main(string[] args)
        {
            Console.WriteLine("Enter query API key: ");

            var key = Console.ReadLine();

            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("No key provided, exiting");
            }

            var indexClient = new SearchIndexClient("catalog-prod", "app", new SearchCredentials(key));

            var parameters = new SearchParameters
            {
                SearchMode = SearchMode.All,
                QueryType = QueryType.Full
            };

            bool retry = true;

            while (retry)
            {
                Console.WriteLine("Enter tenant ID:");

                var id = Console.ReadLine();
                var searchText = $"tenantId:{id}";

                Console.WriteLine($"Executing query 'searchMode={parameters.SearchMode}&queryType={parameters.QueryType}&search={searchText}'\n");

                var count = 0;
                var deduplicatedCount = 0;
                var hadMismatch = false;

                // Repeat the attempt 5 times
                for (int i = 0; i < 50; i++)
                {
                    var searchResult = await indexClient.Documents.SearchAsync<Utterance>(searchText, parameters);
                    var results = searchResult.Results as IEnumerable<SearchResult<Utterance>>;

                    while (searchResult.ContinuationToken != null)
                    {
                        searchResult = await indexClient.Documents.ContinueSearchAsync<Utterance>(searchResult.ContinuationToken);
                        results = results.Concat(searchResult.Results);
                    }

                    Console.WriteLine($"{i + 1}: raw results count: {results.Count()}");

                    var deduplicatedResults = new List<Utterance>();

                    foreach (var result in results)
                    {
                        if (!deduplicatedResults.Exists(u => u.IntentId == result.Document.IntentId))
                        {
                            deduplicatedResults.Add(result.Document);
                        }
                    }

                    if (i == 0)
                    {
                        count = results.Count();
                        deduplicatedCount = deduplicatedResults.Count();
                    }

                    var statement = "Previous";

                    if (i == 1)
                    {
                        statement = "Initial";
                    }

                    if (count != results.Count())
                    {
                        hadMismatch = true;
                        Console.WriteLine($"{i + 1}: count mismatch. {statement}: {count}, current: {results.Count()}");
                    }

                    if (deduplicatedCount != deduplicatedResults.Count())
                    {
                        hadMismatch = true;
                        Console.WriteLine($"{i + 1}: deduplicated count mismatch. {statement}: {deduplicatedCount}, current: {deduplicatedResults.Count()}");
                    }

                    count = results.Count();
                    deduplicatedCount = deduplicatedResults.Count();
                }

                if (!hadMismatch)
                {
                    Console.WriteLine($"No mismatch was found with tenant '{id}'");
                }

                Console.WriteLine("\nTry another tenant? (y/n)");

                var repeat = Console.ReadLine();

                if (repeat != "y")
                {
                    retry = false;
                }
            }
        }
    }
}
