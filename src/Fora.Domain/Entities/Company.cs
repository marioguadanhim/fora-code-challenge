namespace Fora.Domain.Entities
{
    public class Company
    {
        public Company()
        {
            CompanyNetIncomeLoss = [];
        }

        public int Cik { get; set; }

        public string Name { get; set; } = string.Empty;

        public virtual ICollection<CompanyNetIncomeLoss> CompanyNetIncomeLoss { get; set; }
    }
}
