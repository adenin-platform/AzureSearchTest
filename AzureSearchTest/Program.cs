using adenin.IntentRecognizer.Store.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.IO;
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

            Console.WriteLine("Run utterance comparison test? (y/n)");

            var runTest = Console.ReadLine();

            if (runTest == "y")
            {
                var utterances = (await File.ReadAllTextAsync("tenant916_utterances.txt")).Split('\n');
                var count = 1;

                Console.WriteLine($"Doing single searches for each of {utterances.Length - 1} IDs in file");

                foreach (var utterance in utterances)
                {
                    if (string.IsNullOrEmpty(utterance)) continue;

                    Console.WriteLine($"Searching for utterance {count++} '{utterance}'...");

                    var singleResult = await indexClient.Documents.SearchAsync<Utterance>($"id:{utterance}", parameters);

                    if (singleResult.Results.Count(r => r.Document.Id == utterance) == 0)
                    {
                        Console.WriteLine($"Utterance '{utterance}' from file was not returned from index search");
                    }
                }

                Console.WriteLine($"\nFetching all results from index at once with continuation token\n");

                var searchResult = await indexClient.Documents.SearchAsync<Utterance>("tenantId:916", parameters);

                var results = searchResult.Results as IEnumerable<SearchResult<Utterance>>;

                while (searchResult.ContinuationToken != null)
                {
                    searchResult = await indexClient.Documents.ContinueSearchAsync<Utterance>(searchResult.ContinuationToken);
                    results = results.Concat(searchResult.Results);
                }

                Console.WriteLine($"Full search total count: {results.Count()}");

                if (results.Count() != utterances.Count())
                {
                    var resultIds = results.Select(r => r.Document.Id);
                    var difference = utterances.Except(resultIds);

                    Console.WriteLine($"Difference count: {difference.Count()}\n");

                    Console.WriteLine("Differing IDs:\n");

                    foreach (var diff in difference)
                    {
                        Console.WriteLine(diff);
                    }

                    Console.WriteLine($"\nFetching all results from index at once with continuation for second time\n");

                    var searchResult2 = await indexClient.Documents.SearchAsync<Utterance>("tenantId:916", parameters);
                    var results2 = searchResult.Results as IEnumerable<SearchResult<Utterance>>;

                    while (searchResult2.ContinuationToken != null)
                    {
                        searchResult2 = await indexClient.Documents.ContinueSearchAsync<Utterance>(searchResult2.ContinuationToken);
                        results2 = results2.Concat(searchResult2.Results);
                    }

                    var resultIds2 = results2.Select(r => r.Document.Id);

                    Console.WriteLine($"Total count second attempt: {results2.Count()}");

                    var difference2 = utterances.Except(resultIds2);

                    Console.WriteLine($"Difference count with second attempt: {difference2.Count()}");

                    Console.WriteLine("Differing IDs:\n");

                    foreach (var diff in difference2)
                    {
                        Console.WriteLine(diff);
                    }

                    Console.WriteLine("\nDifference between first and second search result sets:\n");

                    var difference3 = resultIds.Except(resultIds2);

                    foreach (var diff in difference3)
                    {
                        Console.WriteLine(diff);
                    }
                }

                return;
            }

            var retry = true;

            while (retry)
            {
                Console.WriteLine("Enter tenant ID:");

                var id = Console.ReadLine();
                var searchText = $"tenantId:{id}";

                Console.WriteLine($"Executing query 'searchMode={parameters.SearchMode}&queryType={parameters.QueryType}&search={searchText}'\n");

                var count = 0;
                var deduplicatedCount = 0;
                var hadMismatch = false;

                IEnumerable<SearchResult<Utterance>> lastResults = new List<SearchResult<Utterance>>();

                // Repeat the attempt
                for (int i = 0; i < 5; i++)
                {
                    var searchResult = await indexClient.Documents.SearchAsync<Utterance>(searchText, parameters);
                    var results = searchResult.Results as IEnumerable<SearchResult<Utterance>>;

                    while (searchResult.ContinuationToken != null)
                    {
                        searchResult = await indexClient.Documents.ContinueSearchAsync<Utterance>(searchResult.ContinuationToken);
                        results = results.Concat(searchResult.Results);
                    }

                    Console.WriteLine($"\n{i + 1}: raw results count: {results.Count()}");

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

                    var currentMismatch = false;

                    if (count != results.Count())
                    {
                        hadMismatch = true;
                        currentMismatch = true;
                        Console.WriteLine($"{i + 1}: count mismatch. {statement}: {count}, current: {results.Count()}");
                    }

                    if (deduplicatedCount != deduplicatedResults.Count())
                    {
                        hadMismatch = true;
                        currentMismatch = true;
                        Console.WriteLine($"{i + 1}: deduplicated count mismatch. {statement}: {deduplicatedCount}, current: {deduplicatedResults.Count()}");
                    }

                    if (currentMismatch && i > 0)
                    {
                        var currentIds = results.Select(r => r.Document.Id);
                        var previousIds = lastResults.Select(r => r.Document.Id);

                        var difference = currentIds.Except(previousIds).ToList();

                        //difference.Sort();

                        if (difference.Count() > 0)
                        {
                            Console.WriteLine($"{i + 1}: The following IDs were not present in both the current and previous results list. Total count: {difference.Count()}");

                            foreach (var intentId in difference)
                            {
                                Console.WriteLine($"\t{intentId}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{i + 1}: No difference was found in the set of IntentIds between this and the previous iteration");
                        }
                    }

                    lastResults = results;
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
