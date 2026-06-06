using EloDoacoes.Models;
using System;
using System.Linq;

namespace EloDoacoes.Data

{
    public static class DbInitializer
    {
        public static void Initialize(EloDoacoesContext context)
        {
            context.Database.EnsureCreated();

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any roles.
            if (context.Roles.Any())
            {
                return;   // DB has been seeded
            }

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

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new User[]
            {
                new User{Name="John",Email="john.doe@example.com",PasswordHash="hashed_password_1",RegistrationDate=DateTime.Parse("2023-01-01"),Role=roles[0]},
                new User{Name="Jane",Email="jane.smith@example.com",PasswordHash="hashed_password_2",RegistrationDate=DateTime.Parse("2023-01-01"),Role=roles[0]}
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            var userJohn = users.First(u => u.Name == "John");
            var userJane = users.First(u => u.Name == "Jane");

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any categories.
            if (context.Categories.Any())
            {
                return;   // DB has been seeded
            }

            var categories = new Category[]
            {
                new Category{Name="Livros"},
                new Category{Name="Móveis"},
                new Category{Name="Roupas"},
                new Category{Name="Alimentos"},
                new Category{Name="Eletrônicos"}
            };
            foreach (Category c in categories)
            {
                context.Categories.Add(c);
            }
            context.SaveChanges();

                var donationCategoryBook = categories.First(c => c.Name == "Livros");
                var donationCategoryFurniture = categories.First(c => c.Name == "Móveis");
                var donationCategoryClothing = categories.First(c => c.Name == "Roupas");
                var donationCategoryFood = categories.First(c => c.Name == "Alimentos");
                var donationCategoryElectronics = categories.First(c => c.Name == "Eletrônicos");

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any donation statuses.
            if (context.DonationStatuses.Any())
            {
                return;   // DB has been seeded
            }

            var donationStatuses = new DonationStatus[]
            {
                new DonationStatus{Name=DonationStatusNameEnum.Available},
                new DonationStatus{Name=DonationStatusNameEnum.Reserved},
            };
            foreach (DonationStatus ds in donationStatuses)
            {
                context.DonationStatuses.Add(ds);
            }
            context.SaveChanges();

            var donationStatusAvailable = donationStatuses.First(ds => ds.Name == DonationStatusNameEnum.Available);
            var donationStatusReserved = donationStatuses.First(ds => ds.Name == DonationStatusNameEnum.Reserved);

            // ## --------------------------------------------------------------------------------------------------------------
            // Look for any donations.
            if (context.Donations.Any())
            {
                return;   // DB has been seeded
            }

            var donations = new Donation[]
            {
                new Donation{Title="Livros de Matemática",Description="Livros em excelente condição",RegistrationDate=DateTime.Parse("2005-09-01"),Category=donationCategoryBook, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Armário",Description="Armário em bom estado",RegistrationDate=DateTime.Parse("2002-09-01"),Category=donationCategoryFurniture, User=userJane, DonationStatus=donationStatusReserved},
                new Donation{Title="Mesa de Jantar",Description="Mesa de jantar de madeira",RegistrationDate=DateTime.Parse("2003-09-01"),Category=donationCategoryFurniture, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Roupas de Inverno",Description="Roupas de inverno em ótimo estado",RegistrationDate=DateTime.Parse("2002-09-01"),Category=donationCategoryClothing, User=userJane, DonationStatus=donationStatusAvailable},
                new Donation{Title="Alimento não perecível",Description="Alimentos não perecíveis dentro do prazo de validade",RegistrationDate=DateTime.Parse("2002-09-01"),Category=donationCategoryFood, User=userJohn, DonationStatus=donationStatusReserved},
                new Donation{Title="Livros de Português",Description="Livros de português em excelente condição",RegistrationDate=DateTime.Parse("2001-09-01"),Category=donationCategoryBook, User=userJane, DonationStatus=donationStatusAvailable},
                new Donation{Title="Televisor",Description="Televisor de tubo",RegistrationDate=DateTime.Parse("2003-09-01"),Category=donationCategoryElectronics, User=userJohn, DonationStatus=donationStatusAvailable},
                new Donation{Title="Roupas Masculinas",Description="Roupas masculinas em ótimo estado",RegistrationDate=DateTime.Parse("2005-09-01"),Category=donationCategoryClothing, User=userJane, DonationStatus=donationStatusAvailable}
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

            var donationImages = new DonationImage[]
            {
                new DonationImage{DonationId=donations[0].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="livros.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[1].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="armario.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[2].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="mesa.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[3].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="roupas.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[4].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="alimentos.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[5].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="livros_portugues.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[6].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="televisor.jpg", DisplayOrder=1},
                new DonationImage{DonationId=donations[7].DonationID, ImageData=new byte[] {0xFF, 0xD8, 0xFF}, ContentType="image/jpeg", FileName="roupas_masculinas.jpg", DisplayOrder=1}
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