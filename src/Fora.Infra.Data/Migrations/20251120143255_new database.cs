using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fora.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class newdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebUser",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebUser", x => x.UserName);
                });

            migrationBuilder.InsertData(
                table: "WebUser",
                columns: new[] { "UserName", "Password", "Role" },
                values: new object[] { "guest", "h1eTenbV4CdRr5fYIYmiq/1mg+6i3WL7ELVS0WsLmB8=", "Guest" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebUser");
        }
    }
}
