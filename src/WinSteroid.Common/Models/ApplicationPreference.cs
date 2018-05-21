using Newtonsoft.Json;

namespace WinSteroid.Common.Models
{
    public class ApplicationPreference
    {
        [JsonProperty("appId")]
        public string AppId { get; set; }

        [JsonProperty("packageName")]
        public string PackageName { get; set; }

        [JsonProperty("icon")]
        public ApplicationIcon Icon { get; set; }

        [JsonProperty("vibration")]
        public VibrationLevel Vibration { get; set; }

        [JsonProperty("muted")]
        public bool Muted { get; set; }
    }
}
