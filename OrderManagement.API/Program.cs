using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.Infrastructure.Services;
using StackExchange.Redis;
using OrderManagement.API.Middleware;
using OrderManagement.Application.Handlers.Auth;
using System.Threading.RateLimiting;
using OrderManagement.Infrastructure.Messaging;
using OrderManagement.Infrastructure.Events;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommandHandler).Assembly));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Management API", Version = "v1" });

    // JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // API Key Authentication
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key Authentication",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityDefinition("ApiSecret", new OpenApiSecurityScheme
    {
        Description = "API Secret Authentication",
        Name = "X-API-Secret",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiSecret"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<OrderDbContext>()
.AddDefaultTokenProviders();

// Configure Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    try
    {
        var redis = ConnectionMultiplexer.Connect(redisConnection);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Redis connection failed: {ex.Message}");
    }
}

// Configure Services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMessageBroker, RabbitMQMessageBroker>(); 
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<RedisCacheService>(); 
builder.Services.AddScoped<OrderStatusChangedEventHandler>();
builder.Services.AddScoped<OrderStatusSubscriber>();


// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new ArgumentNullException("JWT:Key configuration is missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"]
    };
});

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("ApiKeyPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-API-Key"].ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 50,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("JwtPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 200,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var subscriber = scope.ServiceProvider.GetRequiredService<OrderStatusSubscriber>();
    await subscriber.SubscribeToOrderStatusChanged();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseRateLimiter();

app.MapControllers();

// Apply database migrations and seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrderDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.Migrate();

        // Seed roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Seed admin user
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed products
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop",
                    Description = "High-performance laptop with latest specifications",
                    Price = 1299.99m,
                    StockQuantity = 50,
                    Category = "Electronics",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Smartphone",
                    Description = "Latest model smartphone with advanced features",
                    Price = 799.99m,
                    StockQuantity = 100,
                    Category = "Electronics",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Headphones",
                    Description = "Wireless noise-cancelling headphones",
                    Price = 199.99m,
                    StockQuantity = 75,
                    Category = "Accessories",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Smart Watch",
                    Description = "Fitness tracking smartwatch with heart rate monitor",
                    Price = 299.99m,
                    StockQuantity = 60,
                    Category = "Wearables",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Tablet",
                    Description = "10-inch tablet with high-resolution display",
                    Price = 499.99m,
                    StockQuantity = 40,
                    Category = "Electronics",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
        // Seed Orders and OrderItems
        if (!context.Orders.Any())
        {
            var orders = new List<OrderManagement.Core.Entities.Order>
            {
                new OrderManagement.Core.Entities.Order
                {
                    Id = Guid.NewGuid(),
                    UserId = "ec069050-5284-4306-aea6-9c65c46538de",
                    TotalAmount = 2299.98m, 
                    Status = OrderStatus.Pending,
                    ShippingAddress = "1234 Elm St, Springfield, IL",
                    PaymentMethod = "Credit Card",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = new Guid("8ee25bfb-5f3a-4783-99aa-6dc076c1977f"), 
                            ProductName = "Laptop",
                            UnitPrice = 1299.99m,
                            Quantity = 1
                        },
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = new Guid("810f3cab-40bd-4d2b-85f7-b4d33a12b137"), 
                            ProductName = "Smartphone",
                            UnitPrice = 799.99m,
                            Quantity = 1
                        }
                    }
                },
                new OrderManagement.Core.Entities.Order
                {
                    Id = Guid.NewGuid(),
                    UserId = "ec069050-5284-4306-aea6-9c65c46538de",
                    TotalAmount = 699.98m, // Örnek toplam tutar (headphones + smart watch)
                    Status = OrderStatus.Pending,
                    ShippingAddress = "5678 Oak St, Springfield, IL",
                    PaymentMethod = "PayPal",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = new Guid("de0ad005-5e4b-49ec-a56d-5ec6d6fab8d6"), 
                            ProductName = "Headphones",
                            UnitPrice = 199.99m,
                            Quantity = 1
                        },
                        new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId =  new Guid("46e33173-45b6-4c12-91b3-c5c2aeaac9ae"),
                            ProductName = "Smart Watch",
                            UnitPrice = 299.99m,
                            Quantity = 1
                        }
                    }
                }
            };

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();
        }

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
