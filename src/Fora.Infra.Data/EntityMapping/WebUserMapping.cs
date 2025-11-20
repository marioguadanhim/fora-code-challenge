using Fora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fora.Infra.Data.EntityMapping;

public class WebUserMapping : IEntityTypeConfiguration<WebUser>
{
    public void Configure(EntityTypeBuilder<WebUser> builder)
    {
        builder.HasKey(x => x.UserName);

        builder.Property(x => x.UserName).HasMaxLength(200).IsRequired(true);
        builder.Property(x => x.Password).HasMaxLength(200).IsRequired(true);
        builder.Property(x => x.Role).HasMaxLength(200).IsRequired(true);

        builder.ToTable("WebUser");
    }
}