# Adega Royal API - Architecture Documentation

## Overview

This document describes the architecture of the Adega Royal API, a modern e-commerce backend built with .NET 10 and C# 12, designed for React Native mobile application consumption.

## Technology Stack

- **Runtime**: .NET 10
- **Language**: C# 12 (with Primary Constructors)
- **ORM**: Entity Framework Core 10
- **Database**: SQL Server
- **Authentication**: Keycloak (JWT Bearer)
- **API Documentation**: Swagger/OpenAPI
- **Serialization**: System.Text.Json (CamelCase for JS compatibility)

## Architecture Layers

### 1. **Entities (Domain Model)**
Located in `Entities/` folder - core business objects:
- `Category`: Product categorization (Red Wine, White Wine, Beer, etc.)
- `Product`: Catalog items with pricing and stock management
- `Order`: Customer purchase transactions
- `OrderItem`: Individual items within an order

**Key Features**:
- Uses primary constructors for dependency injection
- Immutable IDs (init-only properties)
- UTC DateTime handling
- Navigation properties for relationships

### 2. **DTOs (Data Transfer Objects)**
Located in `DTOs/` folder - prevents circular reference errors and controls API contracts:
- `CategoryDto` / `CreateCategoryDto`
- `ProductDto` / `CreateProductDto` / `UpdateProductDto`
- `OrderDto` / `CreateOrderDto`
- `OrderItemDto` / `CreateOrderItemDto`

**Benefits**:
- Decouples API contracts from domain models
- Prevents JSON serialization cycles
- Clear input/output validation
- React Native-friendly format

### 3. **Services (Business Logic)**
Located in `Services/` folder - implements core functionality:

#### `CategoryService`
- Manages product categories
- CRUD operations with proper validation

#### `ProductService`
- Catalog management
- Stock validation and deduction
- Category relationship handling

#### `OrderService`
- Order creation with atomic transactions
- Stock validation before order creation
- User-specific order filtering
- Status management

**Design Pattern**: Interface-based with primary constructor dependency injection

### 4. **Controllers (API Endpoints)**
Located in `Controllers/` folder - RESTful API endpoints:

#### `CategoriesController`
- `GET /api/categories` - List all categories [Anonymous]
- `GET /api/categories/{id}` - Get category details [Anonymous]
- `POST /api/categories` - Create category [Admin]
- `PUT /api/categories/{id}` - Update category [Admin]
- `DELETE /api/categories/{id}` - Delete category [Admin]

#### `ProductsController`
- `GET /api/products` - List all products with optional category filter [Anonymous]
- `GET /api/products/{id}` - Get product details [Anonymous]
- `POST /api/products` - Create product [Admin]
- `PUT /api/products/{id}` - Update product [Admin]
- `DELETE /api/products/{id}` - Delete product [Admin]

#### `OrdersController`
- `GET /api/orders` - List current user's orders [Authorized]
- `GET /api/orders/{id}` - Get specific order [Authorized]
- `POST /api/orders` - Create new order with stock validation [Authorized]
- `PATCH /api/orders/{id}/status` - Update order status [Admin]
- `DELETE /api/orders/{id}` - Cancel pending order [Authorized]

### 5. **Data Access (DbContext)**
Located in `Data/AppDbContext.cs`:
- Fluent API configurations for all entities
- Cascade delete policies
- Index optimization
- Precision decimal handling (18,2) for pricing

**Key Configurations**:
```csharp
// Prevents orphaned products when category is deleted
.OnDelete(DeleteBehavior.Restrict);

// Automatically deletes items when order is deleted
.OnDelete(DeleteBehavior.Cascade);
```

### 6. **Enums**
Located in `Enums/OrderStatus.cs`:
- `Pending` (0) - New order awaiting payment
- `Paid` (1) - Payment confirmed
- `Shipped` (2) - In transit
- `Delivered` (3) - Received by customer
- `Cancelled` (4) - Customer cancelled
- `Returned` (5) - Return processing

## Security Model

### Authentication
- **Provider**: Keycloak (OAuth 2.0 / OpenID Connect)
- **Token Type**: JWT Bearer
- **Authority**: `http://localhost:8080/realms/adega-royal`
- **Audience**: `account`

### Authorization
- **[AllowAnonymous]**: Public product/category browsing
- **[Authorize]**: Authenticated users (order management)
- **[Authorize(Roles = "admin")]**: Administrative operations

### User Identification
Orders are filtered by user's JWT `sub` claim or `nameidentifier`:
```csharp
private string? GetCurrentUserId()
{
    return User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
```

## Data Validation

### Stock Management
```csharp
// Before order creation
if (!hasStock) throw new InvalidOperationException($"Insufficient stock for product {productId}");

// Atomic deduction
product.StockQuantity -= quantity;
await context.SaveChangesAsync();
```

### Order Deletion
- Only pending orders can be deleted
- Non-admin users can only delete their own orders

## JSON Serialization

**Settings in Program.cs**:
```csharp
options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
```

**Example Response** (React Native friendly):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Vinho Tinto Reserva",
  "price": 99.90,
  "stockQuantity": 50,
  "category": {
    "id": "...",
    "name": "Red Wines"
  },
  "createdAt": "2026-04-23T10:30:00Z"
}
```

## CORS Configuration

Enabled in `Program.cs` for mobile emulator testing:
```csharp
options.AddPolicy("AllowAll", corsPolicyBuilder =>
{
    corsPolicyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});
```

## Database Indexes

Optimized for performance:
- `IX_Product_CategoryId` - Fast category filtering
- `IX_Product_Name` - Product search
- `IX_Order_UserId` - User order retrieval
- `IX_Order_CreatedAt` - Temporal queries
- `IX_OrderItem_OrderId` - Order item lookup
- `IX_OrderItem_ProductId` - Product item tracking

## Error Handling

**Standard Error Responses**:
```json
{
  "message": "Product not found"
}
```

**HTTP Status Codes**:
- `200 OK` - Successful GET request
- `201 Created` - Successful resource creation
- `204 No Content` - Successful deletion
- `400 Bad Request` - Validation error or insufficient stock
- `401 Unauthorized` - Missing or invalid authentication
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource doesn't exist
- `500 Internal Server Error` - Server error

## Database Migration Workflow

### Prerequisites
```bash
# Ensure SQL Server is running
# Update appsettings.json with connection string
```

### Commands
```bash
# Create initial migration
dotnet ef migrations add InitialAdegaroyalMigration

# Apply migration to database
dotnet ef database update

# Revert last migration
dotnet ef migrations remove
```

## Performance Considerations

1. **Eager Loading**: Products and categories are loaded with orders using `.Include()`
2. **Pagination**: Consider implementing for large product lists
3. **Caching**: Category list is ideal for client-side caching
4. **Decimal Precision**: 18,2 precision handles currency correctly

## Future Enhancements

- [ ] Pagination for product listings
- [ ] Advanced search/filtering
- [ ] Product reviews and ratings
- [ ] Wishlist functionality
- [ ] Payment gateway integration
- [ ] Inventory alerts
- [ ] Order notifications (SignalR)
- [ ] Rate limiting
