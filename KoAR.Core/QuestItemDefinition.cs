using System.Text.Json.Serialization;

namespace KoAR.Core
{
    public class QuestItemDefinition
    {
        public uint Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("internal_name")]
        public string InternalName { get; set; } = string.Empty;
    }
}
