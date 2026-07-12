using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EloDoacoes.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // GHOST BASELINE MIGRATION:
            // All DDL code inside Up() has been commented out because the database tables
            // ('user', 'donation', 'reservation', etc.) already exist in the database.
            // Applying this migration will create the '__EFMigrationsHistory' tracking table
            // and record 'InitialCreate' as applied without altering existing tables or data.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Empty down method for baseline
        }
    }
}
