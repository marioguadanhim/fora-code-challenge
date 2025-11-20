using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fora.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class initdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Cik = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Cik);
                });

            migrationBuilder.CreateTable(
                name: "CompanyNetIncomeLoss",
                columns: table => new
                {
                    CompanyNetIncomeLossId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cik = table.Column<int>(type: "int", nullable: false),
                    LossValue = table.Column<long>(type: "bigint", nullable: false),
                    LossFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LossFrame = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyNetIncomeLoss", x => x.CompanyNetIncomeLossId);
                    table.ForeignKey(
                        name: "FK_CompanyNetIncomeLoss_Company_Cik",
                        column: x => x.Cik,
                        principalTable: "Company",
                        principalColumn: "Cik",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Company_Cik",
                table: "Company",
                column: "Cik",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_Name",
                table: "Company",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyNetIncomeLoss_Cik",
                table: "CompanyNetIncomeLoss",
                column: "Cik");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyNetIncomeLoss");

            migrationBuilder.DropTable(
                name: "Company");
        }
    }
}
