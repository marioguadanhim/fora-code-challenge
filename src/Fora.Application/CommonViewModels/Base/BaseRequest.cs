using System.Text.Json.Serialization;

namespace Fora.Application.CommonViewModels.Base;

public abstract class BaseRequest
{
    [JsonIgnore]
    public Guid RequestId { get; set; } = Guid.NewGuid();
}

