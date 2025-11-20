namespace Fora.Domain.Entities
{
    public class CompanyNetIncomeLoss
    {
        public CompanyNetIncomeLoss()
        {
            Company = new();
        }

        public Guid CompanyNetIncomeLossId { get; set; }
        public int Cik { get; set; }
        public long LossValue { get; set; }
        public string LossFormat { get; set; } = string.Empty;
        public string LossFrame { get; set; } = string.Empty;

        public virtual Company Company { get; set; }
    }
}
