using Newtonsoft.Json;

namespace HexMaster.Functions.Auth.Model
{
    public sealed class KeyInfoModel
    {
        [JsonProperty("kty")] public string KeyType { get; set; }
        [JsonProperty("use")] public string Usage { get; set; }
        [JsonProperty("kid")] public string kid { get; set; }
        [JsonProperty("x5t")] public string x5t { get; set; }
        [JsonProperty("n")] public string n { get; set; }
        [JsonProperty("e")] public string e { get; set; }
        [JsonProperty("x5c")] public string[] x5c { get; set; }
        [JsonProperty("issuer")] public string Issuer { get; set; }
    }
}
