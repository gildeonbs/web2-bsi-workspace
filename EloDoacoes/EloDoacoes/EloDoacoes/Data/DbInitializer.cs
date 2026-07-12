using EloDoacoes.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EloDoacoes.Data

{
    public static class DbInitializer
    {
        public static void Initialize(EloDoacoesContext context)
        {
            // Transitioned to EF Core Migrations: EnsureCreated() and ad-hoc ExecuteSqlRaw have been removed
            // so schema migrations and concurrency tokens are managed cleanly via EF Core Migrations.

            // Always ensure all categories exist in the database (idempotent check)
            var categoryNames = new[]
            {
                "Livros", "Móveis", "Roupas", "Alimentos", "Eletrônicos",
                "Brinquedos", "Eletrodomésticos", "Higiene e Limpeza", "Material Escolar",
                "Calçados", "Esporte e Lazer", "Ferramentas", "Artigos para Bebês",
                "Saúde e Bem-estar", "Outros"
            };

            foreach (var catName in categoryNames)
            {
                if (!context.Categories.Any(c => c.Name == catName))
                {
                    context.Categories.Add(new Category { Name = catName });
                }
            }
            context.SaveChanges();

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any roles.
            if (!context.Roles.Any())
            {
                var roles = new Role[]
                {
                    new Role{Name=RoleEnum.StandardUser},
                    new Role{Name=RoleEnum.Admin}
                };
                foreach (Role r in roles)
                {
                    context.Roles.Add(r);
                }
                context.SaveChanges();
            }

            // ## --------------------------------------------------------------------------------------------------------------
            // Ensure demo accounts have phone numbers if already seeded
            var existingJohn = context.Users.FirstOrDefault(u => u.Email == "john.doe@example.com" && u.Phone == null);
            if (existingJohn != null) existingJohn.Phone = "(79) 99671-1625";
            var existingJane = context.Users.FirstOrDefault(u => u.Email == "jane.smith@example.com" && u.Phone == null);
            if (existingJane != null) existingJane.Phone = "(79) 99671-1625";
            context.SaveChanges();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var standardRole = context.Roles.First(r => r.Name == RoleEnum.StandardUser);
            var users = new User[]
            {
                new User{Name="John",Email="john.doe@example.com",Phone="(79) 99671-1625",PasswordHash="hashed_password_1",RegistrationDate=DateTime.Parse("2023-01-01"),Role=standardRole},
                new User{Name="Jane",Email="jane.smith@example.com",Phone="(79) 99671-1625",PasswordHash="hashed_password_2",RegistrationDate=DateTime.Parse("2023-01-01"),Role=standardRole}
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            var userJohn = users.First(u => u.Name == "John");
            var userJane = users.First(u => u.Name == "Jane");


            var donationCategoryBook = context.Categories.First(c => c.Name == "Livros");
            var donationCategoryFurniture = context.Categories.First(c => c.Name == "Móveis");
            var donationCategoryClothing = context.Categories.First(c => c.Name == "Roupas");
            var donationCategoryFood = context.Categories.First(c => c.Name == "Alimentos");
            var donationCategoryElectronics = context.Categories.First(c => c.Name == "Eletrônicos");

            // ## --------------------------------------------------------------------------------------------------------------
            // Ensure all DonationStatus enum values exist in the database
            foreach (DonationStatusNameEnum statusEnum in Enum.GetValues(typeof(DonationStatusNameEnum)))
            {
                if (!context.DonationStatuses.Any(ds => ds.Name == statusEnum))
                {
                    context.DonationStatuses.Add(new DonationStatus { Name = statusEnum });
                }
            }
            context.SaveChanges();

            var donationStatusAvailable = context.DonationStatuses.First(ds => ds.Name == DonationStatusNameEnum.Available);
            var donationStatusReserved = context.DonationStatuses.First(ds => ds.Name == DonationStatusNameEnum.Reserved);

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any donations.
            if (context.Donations.Any())
            {
                return;   // DB has been seeded
            }

            var donations = new Donation[]
            {
                new Donation{Title="Livros de Matemática",Description="Livros em excelente condição",RegistrationDate=DateTime.Parse("2023-09-01"),Category=donationCategoryBook, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Armário",Description="Armário em bom estado",RegistrationDate=DateTime.Parse("2023-09-02"),Category=donationCategoryFurniture, User=userJane, DonationStatus=donationStatusReserved},
                new Donation{Title="Mesa de Jantar",Description="Mesa de jantar de madeira",RegistrationDate=DateTime.Parse("2023-09-03"),Category=donationCategoryFurniture, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Roupas de Inverno",Description="Roupas de inverno em ótimo estado",RegistrationDate=DateTime.Parse("2023-09-04"),Category=donationCategoryClothing, User=userJane, DonationStatus=donationStatusAvailable},
                new Donation{Title="Alimento não perecível",Description="Alimentos não perecíveis dentro do prazo de validade",RegistrationDate=DateTime.Parse("2023-09-05"),Category=donationCategoryFood, User=userJohn, DonationStatus=donationStatusReserved},
                new Donation{Title="Livros de Português",Description="Livros de português em excelente condição",RegistrationDate=DateTime.Parse("2023-09-06"),Category=donationCategoryBook, User=userJane, DonationStatus=donationStatusAvailable},
                new Donation{Title="Televisor",Description="Televisor de tubo funcionando perfeitamente",RegistrationDate=DateTime.Parse("2023-09-07"),Category=donationCategoryElectronics, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Roupas Masculinas",Description="Roupas masculinas em ótimo estado",RegistrationDate=DateTime.Parse("2023-09-08"),Category=donationCategoryClothing, User=userJane, DonationStatus=donationStatusAvailable},
                new Donation{Title="Cadeira de Escritório",Description="Cadeira giratória ergonômica em bom estado",RegistrationDate=DateTime.Parse("2023-09-09"),Category=donationCategoryFurniture, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Teclado e Mouse USB",Description="Teclado ABNT2 e mouse óptico sem fio",RegistrationDate=DateTime.Parse("2023-09-10"),Category=donationCategoryElectronics, User=userJane, DonationStatus=donationStatusAvailable},
                new Donation{Title="Cesta Básica Completa",Description="Arroz, feijão, óleo, açúcar e macarrão lacrados",RegistrationDate=DateTime.Parse("2023-09-11"),Category=donationCategoryFood, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Casaco de Frio Infantil",Description="Casaco infantil tamanho 10 seminovo",RegistrationDate=DateTime.Parse("2023-09-12"),Category=donationCategoryClothing, User=userJane, DonationStatus=donationStatusAvailable}
            };
            foreach (Donation d in donations)
            {
                context.Donations.Add(d);
            }
            context.SaveChanges();

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any donation images.
            if (context.DonationImages.Any())
            {
                return;   // DB has been seeded
            }

            byte[] sampleBytes;
            try
            {
                var samplePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "sample.jpg");
                sampleBytes = System.IO.File.ReadAllBytes(samplePath);
            }
            catch
            {
                // Fallback valid 1x1 JPEG bytes
                sampleBytes = new byte[] { 
                    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48, 
                    0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 
                    0x07, 0x07, 0x07, 0x09, 0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12, 
                    0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20, 0x24, 0x2E, 0x27, 0x20, 
                    0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29, 0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 
                    0x39, 0x3D, 0x38, 0x32, 0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 
                    0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00, 0x01, 0x05, 0x01, 0x01, 
                    0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 
                    0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 
                    0x00, 0xBF, 0x00, 0xFF, 0xD9 
                };
            }

            var donationImages = new DonationImage[]
            {
                new DonationImage{DonationId=donations[0].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="livros.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[1].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="armario.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[2].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="mesa.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[3].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="roupas.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[4].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="alimentos.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[5].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="livros_portugues.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[6].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="televisor.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[7].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="roupas_masculinas.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[8].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="cadeira.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[9].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="teclado.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[10].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="cesta.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[11].DonationID, ImageData=sampleBytes, ContentType="image/jpeg", FileName="casaco.jpg", DisplayOrder=1}
            };
            foreach (DonationImage di in donationImages)
            {
                context.DonationImages.Add(di);
            }
            context.SaveChanges();

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any reservations statuses.
            if (context.ReservationsStatuses.Any())
            {
                return;   // DB has been seeded
            }
            var reservationStatuses = new ReservationStatus[]
            {
                new ReservationStatus{Name=ReservationStatusNameEnum.Pending},
                new ReservationStatus{Name=ReservationStatusNameEnum.Confirmed},
                new ReservationStatus{Name=ReservationStatusNameEnum.Cancelled}
            };
            foreach(var rs in reservationStatuses) {
                context.ReservationsStatuses.Add(rs);
            }
            context.SaveChanges();

            var reservationStatusPending = reservationStatuses.First(rs => rs.Name == ReservationStatusNameEnum.Pending);
            var reservationStatusConfirmed = reservationStatuses.First(rs => rs.Name == ReservationStatusNameEnum.Confirmed);
            var reservationStatusCancelled = reservationStatuses.First(rs => rs.Name == ReservationStatusNameEnum.Cancelled);

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any reservations.
            if (context.Reservations.Any())
            {
                return;   // DB has been seeded
            }

            var reservations = new Reservation[]
            {
                new Reservation{ReservationDate=DateTime.Parse("2026-01-10"), Donation=donations[0], ReservationStatus=reservationStatusPending, User=userJane},
                new Reservation{ReservationDate=DateTime.Parse("2026-01-15"), Donation=donations[1], ReservationStatus=reservationStatusConfirmed, User=userJohn},
                new Reservation{ReservationDate=DateTime.Parse("2026-01-20"), Donation=donations[2], ReservationStatus=reservationStatusCancelled, User=userJane},
                new Reservation{ReservationDate=DateTime.Parse("2026-01-25"), Donation=donations[3], ReservationStatus=reservationStatusPending, User=userJohn},
                new Reservation{ReservationDate=DateTime.Parse("2026-01-30"), Donation=donations[4], ReservationStatus=reservationStatusConfirmed, User=userJane},
                new Reservation{ReservationDate=DateTime.Parse("2026-02-05"), Donation=donations[5], ReservationStatus=reservationStatusCancelled, User=userJohn},
                new Reservation{ReservationDate=DateTime.Parse("2026-02-10"), Donation=donations[6], ReservationStatus=reservationStatusPending, User=userJane},
                new Reservation{ReservationDate=DateTime.Parse("2026-02-15"), Donation=donations[7], ReservationStatus=reservationStatusConfirmed, User=userJohn}
            };
            foreach(Reservation r in reservations)
            {
                context.Reservations.Add(r);
            }
            context.SaveChanges();

        }
    }
}