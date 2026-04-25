# Adega Royal API - Implementation Guide

## 🎯 Quick Start Checklist

- [ ] Review this entire document
- [ ] Update `appsettings.json` with your database connection string
- [ ] Run database migrations
- [ ] Start the API
- [ ] Test endpoints with provided HTTP examples
- [ ] Configure React Native client

---

## 📋 Project Summary

You now have a production-ready e-commerce API for the Adega Royal wine shop with:

### ✅ Implemented Features

1. **Domain Entities** (English names)
   - `Category` - Wine types, beer styles, spirit categories
   - `Product` - Individual items with pricing and stock
   - `Order` - Customer purchase transactions
   - `OrderItem` - Line items in orders

2. **Data Transfer Objects (DTOs)**
   - Request/Response separation to prevent circular references
   - Clean API contracts for React Native
   - Validation attributes on input DTOs

3. **Service Layer**
   - `ICategoryService` / `CategoryService` - Category CRUD
   - `IProductService` / `ProductService` - Product management + stock validation
   - `IOrderService` / `OrderService` - Order management with transactional safety

4. **REST API Controllers**
   - `CategoriesController` - Public browse, Admin manage
   - `ProductsController` - Public browse, Admin manage
   - `OrdersController` - User orders with stock validation

5. **Security**
   - Keycloak JWT authentication
   - Role-based authorization (Admin, User)
   - User isolation (users see only their orders)

6. **Database**
   - SQL Server with EF Core Fluent API
   - Optimized indexes for performance
   - Cascade delete policies
   - UTC datetime handling

7. **API Standards**
   - CamelCase JSON for JavaScript/React Native
   - ISO 8601 datetime format
   - Comprehensive error handling
   - OpenAPI/Swagger documentation
   - CORS enabled for mobile emulators

---

## 🚀 Setup Instructions

### 1. Database Configuration

**File**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AdegaroyalDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 2. Create Database Migrations

```bash
cd d:\Dev\AdegaRoyal\api

# Create migration
dotnet ef migrations add InitialAdegaroyalMigration

# Apply to database
dotnet ef database update
```

**Expected Result**: 4 new tables created (Categories, Products, Orders, OrderItems)

### 3. Run the API

```bash
# Development mode with hot reload
dotnet watch run

# Or standard run
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 4. Test API

**Option A**: Use VS Code REST Client
- Install extension: "REST Client" by Huachao Mao
- Open `AdegaRoyal.http`
- Click "Send Request" on any test

**Option B**: Use curl
```bash
curl http://localhost:5000/api/categories
curl http://localhost:5000/health
```

**Option C**: Use Swagger UI
- Navigate to: `http://localhost:5000/swagger`
- Interactive endpoint testing

---

## 📁 Project Structure

```
KeycloakAuth/
├── Entities/                    # Domain model
│   ├── Category.cs             # Product category entity
│   ├── Product.cs              # Catalog item entity
│   ├── Order.cs                # Purchase order entity
│   ├── OrderItem.cs            # Order line item entity
│   └── (existing User, TaskItem)
│
├── DTOs/                        # Data transfer objects
│   ├── CategoryDto.cs
│   ├── CreateCategoryDto.cs
│   ├── ProductDto.cs
│   ├── CreateProductDto.cs
│   ├── UpdateProductDto.cs
│   ├── OrderDto.cs
│   ├── CreateOrderDto.cs
│   ├── OrderItemDto.cs
│   └── CreateOrderItemDto.cs
│
├── Services/                    # Business logic
│   ├── ICategoryService.cs
│   ├── CategoryService.cs
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IOrderService.cs
│   ├── OrderService.cs
│   └── (existing Keycloak/Task services)
│
├── Controllers/                 # API endpoints
│   ├── CategoriesController.cs
│   ├── ProductsController.cs
│   ├── OrdersController.cs
│   └── (existing AdminController, UserControllers)
│
├── Data/                        # Data access
│   ├── AppDbContext.cs         # Updated with new entities
│   └── (migrations folder)
│
├── Enums/                       # Constants
│   └── OrderStatus.cs          # Order status enumeration
│
├── Program.cs                   # Updated DI configuration
├── ARCHITECTURE.md              # Architecture documentation
├── MIGRATION_INSTRUCTIONS.md    # Database setup guide
├── AdegaRoyal.http              # HTTP test examples
└── README.md
```

---

## 🔗 API Endpoints Reference

### Public Endpoints (No Authentication)

```
GET    /api/categories              → List all categories
GET    /api/categories/{id}         → Get category details
GET    /api/products                → List products (with optional categoryId filter)
GET    /api/products/{id}           → Get product details
GET    /health                      → Health check
```

### Protected Endpoints (Authentication Required)

```
GET    /api/orders                  → User's orders
GET    /api/orders/{id}             → User's specific order
POST   /api/orders                  → Create new order (stock validated)
DELETE /api/orders/{id}             → Cancel pending order
GET    /users/me                    → Current user info
```

### Admin Endpoints

```
POST   /api/categories              → Create category
PUT    /api/categories/{id}         → Update category
DELETE /api/categories/{id}         → Delete category

POST   /api/products                → Create product
PUT    /api/products/{id}           → Update product
DELETE /api/products/{id}           → Delete product

PATCH  /api/orders/{id}/status      → Update order status
```

---

## 🔐 Security Implementation

### Authentication Flow

1. User logs in via Keycloak
2. Receives JWT token with `sub` claim (user ID)
3. Includes token in `Authorization: Bearer {token}` header
4. API validates token signature and expiration
5. Extracts user ID from `sub` claim for order filtering

### Authorization Rules

| Endpoint | Anonymous | User | Admin |
|----------|-----------|------|-------|
| GET Categories | ✅ | ✅ | ✅ |
| GET Products | ✅ | ✅ | ✅ |
| GET Orders | ❌ | ✅* | ✅** |
| Create Order | ❌ | ✅ | ✅ |
| POST Products | ❌ | ❌ | ✅ |
| Admin Status Update | ❌ | ❌ | ✅ |

*Users see only their orders  
**Admins see all orders

---

## 💾 Stock Management

### Stock Validation Process

1. **Before Order Creation**
   ```
   User submits order → 
   Check all items have sufficient stock →
   Calculate total amount →
   Create order record
   ```

2. **Automatic Stock Deduction**
   ```
   For each order item:
   product.StockQuantity -= item.Quantity
   ```

3. **Error Handling**
   ```json
   {
     "message": "Insufficient stock for product ID {id}"
   }
   ```

### Example Response (Successful Order)

```json
HTTP/1.1 201 Created
Content-Type: application/json

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "keycloak-user-123",
  "createdAt": "2026-04-23T15:30:45Z",
  "totalAmount": 499.80,
  "status": "Pending",
  "notes": null,
  "orderItems": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440111",
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2,
      "unitPrice": 199.90,
      "createdAt": "2026-04-23T15:30:45Z",
      "product": {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Vinho Tinto Premium",
        "price": 199.90,
        "stockQuantity": 48,
        "imageUrl": "https://...",
        "categoryId": "..."
      }
    }
  ]
}
```

---

## 📱 React Native Integration

### Installation

```bash
npm install axios
# or
npm install fetch  # built-in
```

### Basic Usage

```typescript
import axios from 'axios';

const API_URL = 'http://192.168.1.100:5000'; // Your API IP

// Create client with automatic header injection
const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token interceptor
apiClient.interceptors.request.use((config) => {
  const token = getStoredToken(); // Your token from Keycloak
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

### Fetch Products

```typescript
const fetchProducts = async () => {
  try {
    const response = await apiClient.get('/api/products');
    return response.data; // Already camelCase!
  } catch (error) {
    console.error('Failed to fetch products:', error);
  }
};
```

### Get User Orders

```typescript
const fetchMyOrders = async () => {
  try {
    const response = await apiClient.get('/api/orders');
    return response.data;
  } catch (error) {
    console.error('Failed to fetch orders:', error.response?.data);
  }
};
```

### Create Order

```typescript
const createOrder = async (items) => {
  try {
    const response = await apiClient.post('/api/orders', {
      items,
      notes: 'Optional delivery instructions',
    });
    return response.data;
  } catch (error) {
    if (error.response?.status === 400) {
      // Stock error or validation error
      console.error(error.response.data.message);
    }
    throw error;
  }
};
```

### Usage Example

```typescript
// Screen component
export const ProductsScreen = () => {
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);

  useEffect(() => {
    fetchProducts().then(setProducts);
  }, []);

  const handleAddToCart = (product) => {
    setCart([...cart, { productId: product.id, quantity: 1 }]);
  };

  const handleCheckout = async () => {
    try {
      const order = await createOrder(cart);
      Alert.alert('Success', `Order #${order.id} created!`);
      setCart([]);
    } catch (error) {
      Alert.alert('Error', error.message);
    }
  };

  return (
    <ScrollView>
      {products.map((product) => (
        <ProductCard
          key={product.id}
          product={product}
          onAddToCart={handleAddToCart}
        />
      ))}
      <Button title="Checkout" onPress={handleCheckout} />
    </ScrollView>
  );
};
```

---

## 🧪 Testing

### Manual Testing

1. **Test Public Access**
   ```bash
   curl http://localhost:5000/api/categories
   # Should return 200 with categories
   ```

2. **Test Authentication Required**
   ```bash
   curl http://localhost:5000/api/orders
   # Should return 401 Unauthorized
   
   curl -H "Authorization: Bearer YOUR_TOKEN" \
     http://localhost:5000/api/orders
   # Should return 200 with user's orders
   ```

3. **Test Stock Validation**
   - Create product with stockQuantity: 1
   - Attempt to order quantity: 2
   - Should receive 400 error: "Insufficient stock"

4. **Test User Isolation**
   - User A creates order
   - User B tries to GET order with User A's order ID
   - Should return 404 (not found)

### Automated Testing

Create `Tests/OrderServiceTests.cs`:

```csharp
[TestFixture]
public class OrderServiceTests
{
    private AppDbContext _context;
    private OrderService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new OrderService(_context);
    }

    [Test]
    public async Task CreateOrder_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var product = new Product(..., stockQuantity: 1);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 }
            }
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateOrderAsync("user-123", dto)
        );
    }
}
```

---

## 🛠️ Troubleshooting

### Issue: "Cannot connect to database"

**Solution**:
```bash
# Verify connection string in appsettings.json
# Check if SQL Server is running
# For SQL Server 2019+: check local services (services.msc on Windows)
# For Docker: docker ps | grep mssql
```

### Issue: "Migration not found"

**Solution**:
```bash
# Ensure you're in the correct directory
cd d:\Dev\AdegaRoyal\api

# Reinstall EF tools if needed
dotnet tool install -g dotnet-ef

# Try again
dotnet ef database update
```

### Issue: "CORS error from React Native"

**Solution**: 
- CORS is already enabled in `Program.cs`
- Ensure API is running: `dotnet run`
- Check emulator can reach your API IP: `ping 192.168.1.100`

### Issue: "401 Unauthorized on protected endpoint"

**Solution**:
- Verify token is valid and not expired
- Check `Authorization` header format: `Bearer {token}`
- Ensure Keycloak is running and reachable
- Verify token contains `sub` claim

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| `ARCHITECTURE.md` | System design and implementation details |
| `MIGRATION_INSTRUCTIONS.md` | Database setup and migration guide |
| `AdegaRoyal.http` | HTTP test examples for all endpoints |
| `IMPLEMENTATION_GUIDE.md` | This file - quick start and overview |

---

## 🎓 Next Steps

### For React Native Development
1. Set up Keycloak client for mobile
2. Implement JWT token storage (AsyncStorage)
3. Create API service wrapper
4. Build product catalog screen
5. Implement shopping cart
6. Integrate order checkout

### For API Enhancement
- [ ] Add pagination to product listings
- [ ] Implement product search/filtering
- [ ] Add product reviews and ratings
- [ ] Implement wishlist functionality
- [ ] Add order tracking/notifications
- [ ] Implement payment gateway integration
- [ ] Add inventory alerts
- [ ] Add promo code/discount system

### For Production Deployment
1. Configure SSL/HTTPS
2. Set up database backups
3. Implement rate limiting
4. Add request logging
5. Set up monitoring/alerting
6. Deploy to Azure App Service or AWS
7. Configure production Keycloak realm
8. Set up CI/CD pipeline

---

## 📞 Support

For issues or questions:
1. Check the error message in the response
2. Review `ARCHITECTURE.md` for system design
3. Review `MIGRATION_INSTRUCTIONS.md` for database issues
4. Test with `AdegaRoyal.http` examples
5. Check Keycloak and SQL Server are running

---

**Generated**: 2026-04-23  
**Framework**: .NET 10 / C# 12  
**Architecture**: Domain-Driven Design with Dependency Injection  
**Status**: ✅ Ready for Development & Testing
