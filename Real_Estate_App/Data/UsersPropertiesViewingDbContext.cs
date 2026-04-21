using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Models;

namespace Real_Estate_App.Data
{
    public class UsersPropertiesViewingDbContext : DbContext
    {
        public UsersPropertiesViewingDbContext(DbContextOptions<UsersPropertiesViewingDbContext> options) : base(options)
        { 
        }
        public DbSet<Viewing> ViewingSet { get; set; }

        public DbSet<User_Data> UsersandAdminsset { get; set; }

        public DbSet<Property> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Viewing>()
                .HasOne(viewing => viewing.Users)
                .WithMany(users => users.Viewings_list)
                .HasForeignKey(viewing => viewing.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Viewing>()
                .HasOne(viewing => viewing.Properties)
                .WithMany(properties => properties.Viewings_list)
                .HasForeignKey(viewing => viewing.PropertyID)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(p => p.PropertyId);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);
                entity.Property(t => t.Price).HasColumnType("decimal(18,2)");
                entity.HasOne(t => t.Property)
                      .WithMany()
                      .HasForeignKey(t => t.PropertyId);
            });

            // Seed data - example properties
            modelBuilder.Entity<Property>().HasData(
                new Property
                {
                    PropertyId = 1,
                    PropertyName = "Sunny Villa",
                    PropertyAddress = "12 Harbour View Road, Auckland 1010",
                    PropertyBedrooms = 4,
                    PropertyBathrooms = 2,
                    PropertyPets = 3,
                    PropertyGarages = 2,
                    ExtendedDescription = "A spacious family home with stunning harbour views, open-plan living, and a large backyard perfect for entertaining. Recently renovated kitchen with modern appliances.",
                    Price = 985000.00m,
                    PropertyType = "House",
                    IsAvailable = true,
                    NearbySchools = "Auckland Grammar School, Epsom Girls Grammar",
                    NearbyShops = "Newmarket Shopping Centre, Parnell Village"
                },
                new Property
                {
                    PropertyId = 2,
                    PropertyName = "City Central Apartment",
                    PropertyAddress = "45 Queen Street, Auckland CBD 1010",
                    PropertyBedrooms = 2,
                    PropertyBathrooms = 1,
                    PropertyPets = 0,
                    PropertyGarages = 1,
                    ExtendedDescription = "Modern apartment in the heart of the CBD with floor-to-ceiling windows and city views. Building includes gym, pool, and concierge service.",
                    Price = 620000.00m,
                    PropertyType = "Apartment",
                    IsAvailable = true,
                    NearbySchools = "Auckland International College",
                    NearbyShops = "Commercial Bay, Britomart"
                },
                new Property
                {
                    PropertyId = 3,
                    PropertyName = "Cosy Ponsonby Flat",
                    PropertyAddress = "78 Ponsonby Road, Ponsonby 1011",
                    PropertyBedrooms = 1,
                    PropertyBathrooms = 1,
                    PropertyPets = 1,
                    PropertyGarages = 0,
                    ExtendedDescription = "Charming ground-floor flat in the trendy Ponsonby strip. Walking distance to cafes, restaurants, and boutique shops. Sunny aspect with a small private courtyard.",
                    Price = 415000.00m,
                    PropertyType = "Flat",
                    IsAvailable = false,
                    NearbySchools = "Ponsonby Intermediate",
                    NearbyShops = "Ponsonby Central, K Road shops"
                },
                new Property
                {
                    PropertyId = 4,
                    PropertyName = "Devonport Family Home",
                    PropertyAddress = "23 Victoria Road, Devonport 0624",
                    PropertyBedrooms = 5,
                    PropertyBathrooms = 3,
                    PropertyPets = 5,
                    PropertyGarages = 2,
                    ExtendedDescription = "Character villa with modern extension on a large section. Five generous bedrooms, three bathrooms including ensuite. Mature gardens and views to Rangitoto Island.",
                    Price = 1450000.00m,
                    PropertyType = "House",
                    IsAvailable = true,
                    NearbySchools = "Devonport Primary, Takapuna Grammar",
                    NearbyShops = "Devonport Village shops, Takapuna Mall"
                },
                new Property
                {
                    PropertyId = 5,
                    PropertyName = "Mt Eden Townhouse",
                    PropertyAddress = "9B Valley Road, Mt Eden 1024",
                    PropertyBedrooms = 3,
                    PropertyBathrooms = 2,
                    PropertyPets = 2,
                    PropertyGarages = 1,
                    ExtendedDescription = "Contemporary townhouse with open-plan living across two levels. Low-maintenance landscaped garden. Double glazing throughout. Close to Mt Eden village and train station.",
                    Price = 870000.00m,
                    PropertyType = "Townhouse",
                    IsAvailable = true,
                    NearbySchools = "Mt Eden Normal Primary, Kowhai Intermediate",
                    NearbyShops = "Mt Eden Village, Dominion Road eateries"
                },
                new Property
                {
                    PropertyId = 6,
                    PropertyName = "Grafton Studio",
                    PropertyAddress = "110 Grafton Road, Grafton 1010",
                    PropertyBedrooms = 0,
                    PropertyBathrooms = 1,
                    PropertyPets = 0,
                    PropertyGarages = 0,
                    ExtendedDescription = "Compact and efficient studio apartment ideal for students or first-home buyers. Located near the University of Auckland and Auckland Hospital. Includes built-in wardrobe and kitchenette.",
                    Price = 285000.00m,
                    PropertyType = "Studio",
                    IsAvailable = false,
                    NearbySchools = "University of Auckland",
                    NearbyShops = "Grafton shops, Newmarket"
                },
                new Property
                {
                    PropertyId = 7,
                    PropertyName = "Remuera Estate",
                    PropertyAddress = "5 Arney Road, Remuera 1050",
                    PropertyBedrooms = 6,
                    PropertyBathrooms = 4,
                    PropertyPets = 4,
                    PropertyGarages = 3,
                    ExtendedDescription = "Prestigious estate home in one of Auckland's premier suburbs. Six bedrooms, four bathrooms, heated pool, and triple garage. Elegant formal and informal living areas with native bush surrounds.",
                    Price = 3200000.00m,
                    PropertyType = "House",
                    IsAvailable = true,
                    NearbySchools = "Remuera Primary, King's College, Diocesan School",
                    NearbyShops = "Remuera Village, Meadowbank Shopping Centre"
                },
                new Property
                {
                    PropertyId = 8,
                    PropertyName = "Hobsonville Point Apartment",
                    PropertyAddress = "32 De Havilland Road, Hobsonville 0618",
                    PropertyBedrooms = 2,
                    PropertyBathrooms = 1,
                    PropertyPets = 1,
                    PropertyGarages = 1,
                    ExtendedDescription = "Brand new apartment in the Hobsonville Point development. Open plan living with quality fixtures. Community parks and coastal walkway on your doorstep.",
                    Price = 545000.00m,
                    PropertyType = "Apartment",
                    IsAvailable = true,
                    NearbySchools = "Hobsonville Point Primary, Hobsonville Point Secondary",
                    NearbyShops = "Hobsonville Point Village, West Harbour shops"
                },
                new Property
                {
                    PropertyId = 9,
                    PropertyName = "Grey Lynn Villa",
                    PropertyAddress = "41 Surrey Crescent, Grey Lynn 1021",
                    PropertyBedrooms = 3,
                    PropertyBathrooms = 1,
                    PropertyPets = 2,
                    PropertyGarages = 1,
                    ExtendedDescription = "Beautifully restored 1910 villa retaining original character features including kauri floors, high ceilings, and ornate fireplaces. Modern kitchen and bathroom. Established gardens.",
                    Price = 1120000.00m,
                    PropertyType = "House",
                    IsAvailable = true,
                    NearbySchools = "Grey Lynn Primary, Western Springs College",
                    NearbyShops = "Grey Lynn shops, Westmere butchery & bakery"
                },
                new Property
                {
                    PropertyId = 10,
                    PropertyName = "Takapuna Beachside Flat",
                    PropertyAddress = "15 The Strand, Takapuna 0622",
                    PropertyBedrooms = 2,
                    PropertyBathrooms = 1,
                    PropertyPets = 1,
                    PropertyGarages = 1,
                    ExtendedDescription = "Light-filled flat just steps from Takapuna Beach. Two double bedrooms, combined lounge/dining with sea glimpses. Shared laundry. Perfect lock-up-and-leave lifestyle.",
                    Price = 680000.00m,
                    PropertyType = "Flat",
                    IsAvailable = false,
                    NearbySchools = "Takapuna Primary, Takapuna Grammar",
                    NearbyShops = "Takapuna town centre, Shore City Mall"
                }
            );

        }
    }
}
