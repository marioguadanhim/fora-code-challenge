using Fora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fora.Infra.Data.EntityMapping
{
    public class CompanyMapping : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(x => x.Cik);

            builder.Property(x => x.Cik)
                 .ValueGeneratedNever();

            builder.HasMany(x => x.CompanyNetIncomeLoss)
            .WithOne(y => y.Company)
            .HasForeignKey(z => z.Cik);

            builder.HasIndex(e => e.Cik).IsUnique();
            builder.HasIndex(e => e.Name);

            builder.Property(e => e.Name).IsRequired().HasMaxLength(500);


            builder.ToTable("Company");
        }
    }
}
