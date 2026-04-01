using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Real_Estate_App.Migrations
{
    /// <inheritdoc />
    public partial class Registration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegisterViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    First_Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Last_Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ConfirmPassword = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterViewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersandAdminsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    First_Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Last_Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersandAdminsset", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegisterViewModel_Email",
                table: "RegisterViewModel",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisterViewModel_UserName",
                table: "RegisterViewModel",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersandAdminsset_Email",
                table: "UsersandAdminsset",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersandAdminsset_UserName",
                table: "UsersandAdminsset",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegisterViewModel");

            migrationBuilder.DropTable(
                name: "UsersandAdminsset");
        }
    }
}
