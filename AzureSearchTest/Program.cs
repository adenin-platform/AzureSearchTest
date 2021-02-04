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
            var key = "6160E12D1A1A79CED8AFD730CF577C9C";
            var indexClient = new SearchIndexClient("catalog-prod", "app", new SearchCredentials(key));

            var parameters = new SearchParameters
            {
                SearchMode = SearchMode.All,
                QueryType = QueryType.Full,
                Top = 1000,
                Skip = 0
            };

            //var utterances = File.ReadAllLines("tenant916_utterances.txt");
            //var count = 0;

            //Console.WriteLine($"Doing single searches for each of {utterances.Length} IDs in file tenant916_utterances.txt");

            //foreach (var utterance in utterances)
            //{
            //    var singleResult = await indexClient.Documents.SearchAsync<Utterance>($"id:{utterance}", parameters);

            //    if (singleResult.Results.Count(r => r.Document.Id == utterance) == 1)
            //    {
            //        count++;
            //    }
            //    else
            //    {
            //        Console.WriteLine($"ERROR: Utterance '{utterance}' from file was not returned from index search");
            //    }
            //}

            //if (count == utterances.Length)
            //{
            //    Console.WriteLine($"Found all {count} utterances from file with index search");
            //}
            //else
            //{
            //    Console.WriteLine($"ERROR: {utterances.Length - count} utterances from file was not returned from index search");
            //}

            Console.WriteLine($"\nFetching all results from index \n");
            var searchResult1 = await indexClient.Documents.SearchAsync<Utterance>("tenantId:214", parameters);
            var results1 = searchResult1.Results as IEnumerable<SearchResult<Utterance>>;
            var resultIds1 = results1.Select(r => r.Document.Id);

            for (int i = 0; i < 100; i++)
            {
                var searchResult2 = await indexClient.Documents.SearchAsync<Utterance>("tenantId:214", parameters);
                var results2 = searchResult2.Results as IEnumerable<SearchResult<Utterance>>;
                var resultIds2 = results2.Select(r => r.Document.Id);
                var differences = resultIds1.Except(resultIds2);

                if (differences.Count() > 0)
                {
                    Console.WriteLine($"{i} ERROR: {differences.Count()} differences");
                }
                else
                {
                    Console.WriteLine($"{i} ok");
                }
            }

            //if (results.Count() != utterances.Count())
            //{
            //    var resultIds = results.Select(r => r.Document.Id);
            //    foreach (var utterance in utterances)
            //    {
            //        if (!resultIds.Contains(utterance))
            //        {
            //            Console.WriteLine($"${utterance} missing\n");
            //        }
            //    }

            //    Console.WriteLine($"\nFetching all results from index at once with continuation for second time\n");

            //    var searchResult2 = await indexClient.Documents.SearchAsync<Utterance>("tenantId:916", parameters);
            //    var results2 = searchResult.Results as IEnumerable<SearchResult<Utterance>>;

            //    while (searchResult2.ContinuationToken != null)
            //    {
            //        searchResult2 = await indexClient.Documents.ContinueSearchAsync<Utterance>(searchResult2.ContinuationToken);
            //        results2 = results2.Concat(searchResult2.Results);
            //    }

            //    var resultIds2 = results2.Select(r => r.Document.Id);

            //    Console.WriteLine($"Total count second attempt: {results2.Count()}");

            //    var difference2 = utterances.Except(resultIds2);

            //    Console.WriteLine($"Difference count with second attempt: {difference2.Count()}");

            //    Console.WriteLine("Differing IDs:\n");

            //    foreach (var diff in difference2)
            //    {
            //        Console.WriteLine(diff);
            //    }

            //    Console.WriteLine("\nDifference between first and second search result sets:\n");

            //    var difference3 = resultIds.Except(resultIds2);

            //    foreach (var diff in difference3)
            //    {
            //        Console.WriteLine(diff);
            //    }
            //}

            return;


        }
    }
}
