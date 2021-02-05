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

            // get first results
            var searchResult1 = await indexClient.Documents.SearchAsync<Utterance>("tenantId:214", parameters);
            var results1 = searchResult1.Results as IEnumerable<SearchResult<Utterance>>;
            var scores1 = results1.Select(r => r.Document.Id); // r.Score.ToString());

            for (int i = 0; i < 100; i++)
            {
                var searchResult2 = await indexClient.Documents.SearchAsync<Utterance>("tenantId:214", parameters);
                var results2 = searchResult2.Results as IEnumerable<SearchResult<Utterance>>;
                var scores2 = results2.Select(r => r.Document.Id); // r.Score.ToString());
                
                if (!scores2.SequenceEqual(scores1))
                {
                    Console.WriteLine($"{i}. scores not equal");
                    var r1 = scores1.ToArray();
                    var r2 = scores2.ToArray();
                    for(int i1=0; i1<r1.Length;i1++)
                    {
                        // if(r1[i1]!=r2[i1]) Console.WriteLine($"  [{i1}]  {r1[i1]}  !=  {r2[i1]}");
                    }
                }
                else
                {
                    Console.WriteLine($"{i}. scores equal");
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
