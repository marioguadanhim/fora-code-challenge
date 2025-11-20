using System.Text.Json.Serialization;

namespace Fora.Domain.ValueObjects
{
    public class EdgarCompanyInfo
    {
        public int Cik { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public InfoFact Facts { get; set; } = new();

        public class InfoFact
        {
            [JsonPropertyName("us-gaap")]
            public InfoFactUsGaap UsGaap { get; set; } = new();
        }

        public class InfoFactUsGaap
        {
            public InfoFactUsGaapNetIncomeLoss NetIncomeLoss { get; set; } = new();
        }

        public class InfoFactUsGaapNetIncomeLoss
        {
            public InfoFactUsGaapIncomeLossUnits Units { get; set; } = new();
        }

        public class InfoFactUsGaapIncomeLossUnits
        {
            public InfoFactUsGaapIncomeLossUnitsUsd[] Usd { get; set; } = Array.Empty<InfoFactUsGaapIncomeLossUnitsUsd>();
        }

        public class InfoFactUsGaapIncomeLossUnitsUsd
        {
            public string Form { get; set; } = string.Empty;

            public string Frame { get; set; } = string.Empty;

            public decimal Val { get; set; }
        }
    }
}