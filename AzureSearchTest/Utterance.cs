using System.Text.RegularExpressions;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace adenin.IntentRecognizer.Store.Models
{
    [SerializePropertyNamesAsCamelCase]
    public sealed class Utterance
    {
        public Utterance(string tenantId, string intentId, string entityType, string container, string content, string json)
        {
            Id = string.Join("=", tenantId, intentId, Regex.Replace(content, "[^A-Za-z0-9]", "_"));
            TenantId = tenantId;
            IntentId = intentId;
            EntityType = entityType;
            Container = container;
            Content = content;
            Json = json;
        }

        [System.ComponentModel.DataAnnotations.Key, IsSearchable]
        public string Id { get; }

        [IsSearchable]
        public string TenantId { get; }

        [IsSearchable]
        public string IntentId { get; }

        [IsSearchable]
        public string EntityType { get; }

        [IsSearchable]
        public string Container { get; }

        [IsSearchable]
        public string Content { get; }

        public string Json { get; }
    }
}
