using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicShop.Common.Models;

namespace MusicShop.Server.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<EmailQueue> EmailQueues => Set<EmailQueue>();

        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<InventoryLedger> InventoryLedgers => Set<InventoryLedger>();

        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Profile
            b.Entity<Profile>(e =>
            {
                e.HasKey(x => x.UserId);
                e.Property(x => x.UserId).ValueGeneratedNever();
                e.HasOne(x => x.User)
                 .WithOne(u => u.Profile)
                 .HasForeignKey<Profile>(x => x.UserId)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Email
            b.Entity<EmailQueue>(e => e.Property(x => x.Status).HasConversion<byte>());

            // Catalog
            b.Entity<Brand>().Property(x => x.Name).IsRequired();
            b.Entity<Category>()
                .HasOne(x => x.Parent).WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Item>(e =>
            {
                e.Property(x => x.Price).HasColumnType("decimal(18,2)");
                e.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
                e.Property(x => x.ItemType).HasConversion<byte>();
                e.Property(x => x.Status).HasConversion<byte>();
                e.HasIndex(x => x.Sku).IsUnique();
                e.Property(x => x.RowVersion).IsRowVersion();
            });

            b.Entity<InventoryLedger>(e => e.Property(x => x.Reason).HasConversion<byte>());

            // Cart
            b.Entity<Cart>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.IsCheckout });
                e.HasIndex(x => new { x.GuestId, x.IsCheckout });
            });

            b.Entity<CartItem>(e =>
            {
                e.HasKey(x => new { x.CartId, x.ItemId });
                e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            });

            // Order
            b.Entity<Order>(e =>
            {
                e.HasIndex(x => x.OrderNumber).IsUnique();
                e.Property(x => x.Status).HasConversion<byte>();
                e.Property(x => x.Total).HasColumnType("decimal(18,2)");
                e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            });

            b.Entity<OrderItem>(e =>
            {
                e.HasKey(x => new { x.OrderId, x.ItemId });
                e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            });

            b.Entity<OrderAddress>().HasKey(x => x.OrderId);

            // Payment
            b.Entity<Payment>(e =>
            {
                e.Property(x => x.Method).HasConversion<byte>();
                e.Property(x => x.Status).HasConversion<byte>();
                e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            });

            // ====== Seed deterministic ======
            var seedDate = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            // Identity Roles
            var adminRoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var userRoleId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var managerRoleId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
            b.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "11111111-1111-1111-1111-111111111111"
                },
                new ApplicationRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
                },
                new ApplicationRole
                {
                    Id = managerRoleId,
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = "32222222-2222-2222-2222-222222222223"
                }
            );

            // Identity Users
            var adminUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var normalUserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var managerUserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

            const string passwordHash = "AQAAAAIAAYagAAAAEIItcWHMUyC+Ay+/CAqMzmzJyqgq5oBEnSPYR6YE+zXuBR8WKV46t7jCwyZSkBUnAw=="; // "abc123"


            b.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = adminUserId,
                    UserName = "admin@musicshop.local",
                    NormalizedUserName = "ADMIN@MUSICSHOP.LOCAL",
                    Email = "admin@musicshop.local",
                    NormalizedEmail = "ADMIN@MUSICSHOP.LOCAL",
                    EmailConfirmed = true,
                    PasswordHash = passwordHash,
                    SecurityStamp = "33333333-3333-3333-3333-333333333333",
                    ConcurrencyStamp = "44444444-4444-4444-4444-444444444444"
                },
                new ApplicationUser
                {
                    Id = normalUserId,
                    UserName = "user@musicshop.local",
                    NormalizedUserName = "USER@MUSICSHOP.LOCAL",
                    Email = "user@musicshop.local",
                    NormalizedEmail = "USER@MUSICSHOP.LOCAL",
                    EmailConfirmed = true,
                    PasswordHash = passwordHash,
                    SecurityStamp = "55555555-5555-5555-5555-555555555555",
                    ConcurrencyStamp = "66666666-6666-6666-6666-666666666666"
                },
                new ApplicationUser
                {
                    Id = managerUserId,
                    UserName = "manager@musicshop.local",
                    NormalizedUserName = "MANAGER@MUSICSHOP.LOCAL",
                    Email = "manager@musicshop.local",
                    NormalizedEmail = "MANAGER@MUSICSHOP.LOCAL",
                    EmailConfirmed = true,
                    PasswordHash = passwordHash,
                    SecurityStamp = "65555555-5555-5555-5555-555555555556",
                    ConcurrencyStamp = "36666666-6666-6666-6666-666666666663"
                }
            );

            // Seed UserRoles
            b.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    UserId = adminUserId,
                    RoleId = adminRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = normalUserId,
                    RoleId = userRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = managerUserId,
                    RoleId = managerRoleId
                }
            );

            // BRANDS
            b.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Yamaha", CreatedAt = seedDate },
                new Brand { Id = 2, Name = "Casio", CreatedAt = seedDate },
                new Brand { Id = 3, Name = "Korg", CreatedAt = seedDate },
                new Brand { Id = 4, Name = "Roland", CreatedAt = seedDate },
                new Brand { Id = 5, Name = "Fender", CreatedAt = seedDate }
            );

            // CATEGORIES
            b.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Piano/Keyboard", CreatedAt = seedDate },
                new Category { Id = 2, Name = "Guitar", CreatedAt = seedDate },
                new Category { Id = 3, Name = "Percussion", CreatedAt = seedDate },
                new Category { Id = 4, Name = "Micro/Audio", CreatedAt = seedDate },
                new Category { Id = 5, Name = "Phụ kiện", CreatedAt = seedDate }
            );

            // ITEMS 
            var i1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var i2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var i3 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var i4 = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var i5 = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var i6 = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var i7 = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var i8 = Guid.Parse("88888888-8888-8888-8888-888888888888");
            var i9 = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var i10 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var i11 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var i12 = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var i13 = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var i14 = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
            var i15 = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
            var i16 = Guid.Parse("12121212-1212-1212-1212-121212121212");
            var i17 = Guid.Parse("34343434-3434-3434-3434-343434343434");
            var i18 = Guid.Parse("56565656-5656-5656-5656-565656565656");
            var i19 = Guid.Parse("78787878-7878-7878-7878-787878787878");
            var i20 = Guid.Parse("90909090-9090-9090-9090-909090909090");

            b.Entity<Item>().HasData(
                // Piano/Keyboard
                new Item
                {
                    Id = i1,
                    Sku = "KB-YAM-E473",
                    Name = "Yamaha PSR-E473",
                    BrandId = 1,
                    CategoryId = 1,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 6990000m,
                    StockQty = 8,
                    ReorderLevel = 2,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i2,
                    Sku = "KB-CAS-CTS1",
                    Name = "Casio CTS-1",
                    BrandId = 2,
                    CategoryId = 1,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 3950000m,
                    StockQty = 12,
                    ReorderLevel = 3,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i3,
                    Sku = "KB-KRG-KROSS2",
                    Name = "Korg Kross 2 61",
                    BrandId = 3,
                    CategoryId = 1,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 15990000m,
                    StockQty = 5,
                    ReorderLevel = 1,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i4,
                    Sku = "KB-ROL-FP10",
                    Name = "Roland FP-10",
                    BrandId = 4,
                    CategoryId = 1,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 14990000m,
                    StockQty = 6,
                    ReorderLevel = 2,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },

                // Guitar
                new Item
                {
                    Id = i5,
                    Sku = "GT-FEN-CD60",
                    Name = "Fender CD-60 V3",
                    BrandId = 5,
                    CategoryId = 2,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 4990000m,
                    StockQty = 10,
                    ReorderLevel = 3,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i6,
                    Sku = "GT-YAM-F310",
                    Name = "Yamaha F310",
                    BrandId = 1,
                    CategoryId = 2,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 3290000m,
                    StockQty = 15,
                    ReorderLevel = 4,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i7,
                    Sku = "GT-FEN-PLAYER-STRAT",
                    Name = "Fender Player Stratocaster",
                    BrandId = 5,
                    CategoryId = 2,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 20990000m,
                    StockQty = 3,
                    ReorderLevel = 1,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i8,
                    Sku = "GT-YAM-PAC012",
                    Name = "Yamaha Pacifica 012",
                    BrandId = 1,
                    CategoryId = 2,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 4990000m,
                    StockQty = 7,
                    ReorderLevel = 2,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },

                // Percussion
                new Item
                {
                    Id = i9,
                    Sku = "DRM-ROL-TD1DMK",
                    Name = "Roland TD-1DMK",
                    BrandId = 4,
                    CategoryId = 3,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 17990000m,
                    StockQty = 2,
                    ReorderLevel = 1,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i10,
                    Sku = "DRM-YAM-DD75",
                    Name = "Yamaha DD-75",
                    BrandId = 1,
                    CategoryId = 3,
                    ItemType = ItemType.Instrument,
                    Status = ItemStatus.Active,
                    Price = 6290000m,
                    StockQty = 9,
                    ReorderLevel = 2,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },

                // Micro/Audio
                new Item
                {
                    Id = i11,
                    Sku = "MIC-ROL-VT4",
                    Name = "Roland VT-4 Voice Transformer",
                    BrandId = 4,
                    CategoryId = 4,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 7990000m,
                    StockQty = 4,
                    ReorderLevel = 1,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i12,
                    Sku = "MIC-YAM-MG10XU",
                    Name = "Yamaha MG10XU Mixer",
                    BrandId = 1,
                    CategoryId = 4,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 6290000m,
                    StockQty = 6,
                    ReorderLevel = 2,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i13,
                    Sku = "MIC-ROL-GO:MIXER-PROX",
                    Name = "Roland GO:MIXER PRO-X",
                    BrandId = 4,
                    CategoryId = 4,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 4990000m,
                    StockQty = 8,
                    ReorderLevel = 2,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },

                // Phụ kiện
                new Item
                {
                    Id = i14,
                    Sku = "ACC-YAM-FC5",
                    Name = "Yamaha FC5 Sustain Pedal",
                    BrandId = 1,
                    CategoryId = 5,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 390000m,
                    StockQty = 20,
                    ReorderLevel = 5,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i15,
                    Sku = "ACC-UNI-GUITAR-STRAP",
                    Name = "Dây đeo guitar Universal",
                    BrandId = null,
                    CategoryId = 5,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 150000m,
                    StockQty = 30,
                    ReorderLevel = 10,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i16,
                    Sku = "ACC-UNI-CAPO-A",
                    Name = "Capo guitar hợp kim",
                    BrandId = null,
                    CategoryId = 5,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 120000m,
                    StockQty = 25,
                    ReorderLevel = 8,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i17,
                    Sku = "ACC-UNI-PICK-SET",
                    Name = "Bộ pick guitar (12 chiếc)",
                    BrandId = null,
                    CategoryId = 5,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 60000m,
                    StockQty = 50,
                    ReorderLevel = 15,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i18,
                    Sku = "ACC-UNI-KEYBOARD-STAND-X",
                    Name = "Chân X keyboard",
                    BrandId = null,
                    CategoryId = 5,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 350000m,
                    StockQty = 18,
                    ReorderLevel = 5,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i19,
                    Sku = "ACC-UNI-MIC-BOOM",
                    Name = "Chân micro boom",
                    BrandId = null,
                    CategoryId = 5,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 280000m,
                    StockQty = 22,
                    ReorderLevel = 6,
                    ImageUrl = null,
                    CreatedAt = seedDate
                },
                new Item
                {
                    Id = i20,
                    Sku = "ACC-UNI-GUITAR-BAG-41",
                    Name = "Bao đàn guitar 41 inch",
                    BrandId = null,
                    CategoryId = 5,
                    ItemType = ItemType.Accessory,
                    Status = ItemStatus.Active,
                    Price = 220000m,
                    StockQty = 16,
                    ReorderLevel = 5,
                    ImageUrl = null,
                    CreatedAt = seedDate
                }
            );

            // EMAIL QUEUE (mẫu cho Worker)
            b.Entity<EmailQueue>().HasData(
                new EmailQueue
                {
                    Id = 1,
                    Recipient = "customer@musicshop.local",
                    Subject = "Chào mừng",
                    Content = "<p>Cảm ơn bạn đã đăng ký.</p>",
                    Status = EmailStatus.Queued,
                    RetryCount = 0,
                    MaxRetry = 3,
                    CreatedAt = seedDate
                },
                new EmailQueue
                {
                    Id = 2,
                    Recipient = "admin@musicshop.local",
                    Subject = "Báo cáo hằng ngày",
                    Content = "<p>Báo cáo mẫu.</p>",
                    Status = EmailStatus.Queued,
                    RetryCount = 0,
                    MaxRetry = 3,
                    CreatedAt = seedDate
                }
            );
        }
    }
}
