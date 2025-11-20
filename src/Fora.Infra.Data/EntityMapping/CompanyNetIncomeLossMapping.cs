using Fora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fora.Infra.Data.EntityMapping
{
    public class CompanyNetIncomeLossMapping : IEntityTypeConfiguration<CompanyNetIncomeLoss>
    {
        public void Configure(EntityTypeBuilder<CompanyNetIncomeLoss> builder)
        {
            builder.HasKey(x => x.CompanyNetIncomeLossId);

            builder.HasOne(x => x.Company)
                .WithMany(y => y.CompanyNetIncomeLoss)
                .HasForeignKey(z => z.Cik);

            builder.Property(x => x.LossFormat).IsRequired();
            builder.Property(x => x.LossFrame).IsRequired();

            builder.ToTable("CompanyNetIncomeLoss");
        }
    }
}
