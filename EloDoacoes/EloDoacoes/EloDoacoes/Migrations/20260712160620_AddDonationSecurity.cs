using Microsoft.EntityFrameworkCore.Migrations;

namespace EloDoacoes.Migrations
{
    public partial class AddDonationSecurity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely verify and ensure the RowVersion concurrency token column exists on the donation table.
            // In SQL Server, rowversion columns automatically populate timestamp values for all existing records.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'donation') AND name = 'RowVersion'
                )
                BEGIN
                    ALTER TABLE [donation] ADD [RowVersion] rowversion NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "donation");
        }
    }
}
