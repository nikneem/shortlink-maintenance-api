using System.Collections.Generic;
using Newtonsoft.Json;

namespace HexMaster.Functions.Auth.Model
{
    public sealed class SigningKeysModel
    {
        [JsonProperty("keys")] public List<KeyInfoModel> Keys { get; set; }
    }
}
