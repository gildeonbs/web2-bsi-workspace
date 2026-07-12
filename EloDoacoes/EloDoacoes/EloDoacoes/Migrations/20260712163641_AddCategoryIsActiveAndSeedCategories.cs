using Microsoft.EntityFrameworkCore.Migrations;

namespace EloDoacoes.Migrations
{
    public partial class AddCategoryIsActiveAndSeedCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add non-nullable IsActive column with default value = true (Contoso University pattern)
            // so all existing categories default to active (IsActive = 1).
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "category",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // 2. Seed any missing categories into the database cleanly via idempotent SQL migration commands
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Brinquedos')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Brinquedos', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Eletrodomésticos')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Eletrodomésticos', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Higiene e Limpeza')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Higiene e Limpeza', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Material Escolar')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Material Escolar', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Calçados')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Calçados', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Esporte e Lazer')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Esporte e Lazer', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Ferramentas')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Ferramentas', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Artigos para Bebês')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Artigos para Bebês', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Saúde e Bem-estar')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Saúde e Bem-estar', 1);

                IF NOT EXISTS (SELECT 1 FROM [category] WHERE [Name] = N'Outros')
                    INSERT INTO [category] ([Name], [IsActive]) VALUES (N'Outros', 1);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "category");
        }
    }
}
