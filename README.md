# E-commerce REST API
 
A production-ready, fully-featured e-commerce REST API built with .NET 9, Entity Framework Core, and SQL Server. This project demonstrates advanced backend development patterns, complex database relationships, and real-world business logic.
 
**Status:** Complete | **Latest Version:** 1.0.0
 
---
 
## 📋 Table of Contents
 
- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Database Design](#database-design)
- [Architecture Patterns](#architecture-patterns)
- [Error Handling](#error-handling)
- [Future Enhancements](#future-enhancements)
---
 
## 🎯 Overview
 
This e-commerce API provides a complete solution for managing:
 
- **Product Catalog** — Create, list, search, and filter products
- **Shopping Cart** — Add/remove items, manage quantities, calculate totals
- **Orders** — Create orders from cart, track order status, manage order history
- **Payments** — Process payments, track payment status, handle refunds
- **Inventory Management** — Real-time stock updates, prevent overselling
**Perfect for:**
- E-commerce platforms
- Marketplace applications
- Retail management systems
- Learning advanced backend development
---
 
## ✨ Features
 
### Products
- ✅ Full CRUD operations
- ✅ Search by name
- ✅ Filter by category and price range
- ✅ Stock management
- ✅ Pagination support
### Shopping Cart
- ✅ Customer-specific carts
- ✅ Add/remove items
- ✅ Update quantities
- ✅ Real-time total calculation
- ✅ Price snapshots (for historical accuracy)
### Orders
- ✅ Create orders from cart
- ✅ Order status tracking (Pending → Paid → Shipped → Delivered)
- ✅ Order history by customer
- ✅ Cancel orders
- ✅ Automatic stock reduction
### Payments
- ✅ Process payments
- ✅ Track payment status
- ✅ Payment method support
- ✅ Refund functionality
- ✅ Payment statistics
### Business Logic
- ✅ Prevent overselling (stock validation)
- ✅ Price snapshots at time of order
- ✅ Automatic cart clearing after order creation
- ✅ Stock restoration on order cancellation
---
 
## 🛠️ Tech Stack
 
| Component | Technology | Version |
|-----------|-----------|---------|
| **Runtime** | .NET Core | 9.0 |
| **Language** | C# | 12 |
| **Web Framework** | ASP.NET Core | 9.0 |
| **ORM** | Entity Framework Core | 9.0 |
| **Database** | SQL Server | 2019+ |
| **API Docs** | Swagger/OpenAPI | 3.0 |
| **Logging** | ILogger | Built-in |
| **DI Container** | Built-in | .NET 9 |
 
---
 
## 🚀 Getting Started
 
### Prerequisites
 
- **.NET 9 SDK** — [Download](https://dotnet.microsoft.com/download)
- **SQL Server 2019+** or **SQL Server LocalDB** (included with Visual Studio)
- **Visual Studio 2022** (optional but recommended)
- **Git**
### Installation
 
#### 1. Clone Repository
 
```bash
git clone https://github.com/YOUR-USERNAME/ecommerce-api.git
cd ecommerce-api/EcommerceApi
```
 
#### 2. Install Dependencies
 
```bash
dotnet restore
```
 
#### 3. Setup Database
 
```bash
# Create database and tables
dotnet ef migrations add InitialCreate
dotnet ef database update
```
 
#### 4. Run Application
 
```bash
dotnet run
```
 
API starts at: `https://localhost:7022`
 
#### 5. Access Swagger UI
 
Open browser: `https://localhost:7022`
 
---
 
## 📁 Project Structure
 
```
EcommerceApi/
├── Models/
│   ├── Product.cs
│   ├── Cart.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Payment.cs
├── DTOs/
│   ├── ProductDto.cs
│   ├── CartDto.cs
│   ├── OrderDto.cs
│   └── PaymentDto.cs
├── Controllers/
│   ├── ProductsController.cs
│   ├── CartsController.cs
│   ├── OrdersController.cs
│   └── PaymentsController.cs
├── Data/
│   └── EcommerceDbContext.cs
├── Migrations/
│   └── (auto-generated)
├── Program.cs
├── appsettings.json
└── README.md
```
 
---
 
## 🔌 API Endpoints
 
### Products
 
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products (paginated) |
| GET | `/api/products/{id}` | Get product by ID |
| GET | `/api/products/search/{query}` | Search products by name |
| GET | `/api/products/category/{category}` | Filter by category |
| POST | `/api/products` | Create product (admin) |
| PUT | `/api/products/{id}` | Update product (admin) |
| DELETE | `/api/products/{id}` | Delete product (admin) |
 
### Shopping Cart
 
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/carts/{customerId}` | Get customer's cart |
| POST | `/api/carts/add` | Add item to cart |
| PUT | `/api/carts/items/{itemId}` | Update item quantity |
| DELETE | `/api/carts/items/{itemId}` | Remove item from cart |
| DELETE | `/api/carts/{customerId}` | Clear entire cart |
 
### Orders
 
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders/customer/{customerId}` | Get customer's orders |
| GET | `/api/orders/{orderId}` | Get order details |
| GET | `/api/orders/status/{status}` | Get orders by status |
| POST | `/api/orders/create` | Create order from cart |
| PUT | `/api/orders/{orderId}/status` | Update order status |
| POST | `/api/orders/{orderId}/cancel` | Cancel order |
 
### Payments
 
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/payments/order/{orderId}` | Get order payments |
| GET | `/api/payments/{paymentId}` | Get payment details |
| POST | `/api/payments/process/{orderId}` | Process payment |
| POST | `/api/payments/{paymentId}/refund` | Refund payment |
| GET | `/api/payments/stats/daily` | Get daily stats |
 
---
 
### Example: Create Order
 
**Request:**
```http
POST /api/orders/create HTTP/1.1
Content-Type: application/json
 
{
  "customerId": "customer123",
  "customerEmail": "customer@example.com",
  "shippingAddress": "123 Main St, City, State 12345",
  "notes": "Deliver before 5 PM"
}
```
 
**Response (201 Created):**
```json
{
  "id": 1,
  "customerId": "customer123",
  "status": "Pending",
  "totalAmount": 299.97,
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Laptop",
      "quantity": 1,
      "priceAtOrderTime": 299.97,
      "lineTotal": 299.97
    }
  ],
  "payments": [],
  "itemCount": 1,
  "isPaid": false,
  "createdAt": "2025-04-11T10:30:00Z"
}
```
 
---
 
## 🗄️ Database Design
 
### Entity Relationship Diagram
 
```
Product (1) ──────── (M) CartItem
  ↑                        ↑
  │                        │
  └────────────────────────┘
                (M)
              
Product (1) ──────── (M) OrderItem  ──────── (1) Order
                                      │
                                      │
                                   (M)
                                      ↓
                                  Payment
 
Cart (1) ──────── (M) CartItem
  │
  └─ CustomerId (unique)
```
 
### Key Tables
 
| Table | Purpose | Key Fields |
|-------|---------|-----------|
| **Products** | Product catalog | Id, Name, Price, StockQuantity |
| **Carts** | Shopping carts | Id, CustomerId (unique) |
| **CartItems** | Cart line items | Id, CartId, ProductId, Quantity |
| **Orders** | Customer orders | Id, CustomerId, Status, TotalAmount |
| **OrderItems** | Order line items | Id, OrderId, ProductId, Quantity |
| **Payments** | Payment transactions | Id, OrderId, Amount, Status |
 
### Indexes for Performance
 
- `IDX_Products_Category` — Fast category filtering
- `IDX_Products_IsActive` — Show/hide products
- `IDX_Orders_CustomerId` — Fetch customer orders
- `IDX_Orders_Status` — Filter by order status
- `IDX_Payments_Status` — Track payment status
---
 
## 🏗️ Architecture Patterns
 
### 1. **Dependency Injection**
 
```csharp
// EF Core DbContext injected into controllers
public ProductsController(EcommerceDbContext context, ILogger<ProductsController> logger)
{
    _context = context;
    _logger = logger;
}
```
 
**Benefits:**
- Loose coupling
- Easy testing (mock DbContext)
- Centralized configuration
---
 
### 2. **Repository Pattern (via EF Core)**
 
```csharp
// EF Core replaces traditional repository
var products = await _context.Products
    .Where(p => p.Category == "Electronics")
    .ToListAsync();
```
 
**Benefits:**
- Abstraction between business logic and data
- Testable queries
- Easy to change database provider
---
 
### 3. **DTO Pattern (Data Transfer Objects)**
 
```csharp
// Separate API contract from database model
public class ProductCreateRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
 
// Maps to Product entity
var product = new Product { Name = request.Name, ... };
```
 
**Benefits:**
- API contract independent of DB schema
- Input validation separate from model
- Hide internal fields
---
 
### 4. **Async/Await for Scalability**
 
```csharp
// Non-blocking I/O
public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest request)
{
    var order = new Order { ... };
    await _context.SaveChangesAsync(); // Non-blocking
    return Ok(order);
}
```
 
**Benefits:**
- Handles more concurrent requests
- Better resource utilization
- Improved performance under load
---
 
### 5. **Business Logic Encapsulation**
 
```csharp
// Methods in models for business logic
public class Order
{
    public int GetItemCount() => Items.Sum(i => i.Quantity);
    public bool IsPaid() => Status == OrderStatus.Paid;
}
```
 
**Benefits:**
- Logic stays with data
- Reusable across controllers
- Easier to maintain
---
 
## ⚠️ Error Handling
 
The API returns proper HTTP status codes:
 
| Status | Meaning | Example |
|--------|---------|---------|
| **200** | OK | Product fetched successfully |
| **201** | Created | Order created |
| **400** | Bad Request | Quantity is negative |
| **404** | Not Found | Product doesn't exist |
| **500** | Server Error | Database connection failed |
 
**Error Response Format:**
```json
{
  "message": "User-friendly error message",
  "error": "Detailed technical error (in dev mode)"
}
```
 
---
 
## 🔐 Security Considerations
 
This is a demo API. For production, add:
 
- [ ] **Authentication** — JWT tokens
- [ ] **Authorization** — Role-based access (admin, user)
- [ ] **Rate Limiting** — Prevent abuse
- [ ] **Input Validation** — Sanitize all inputs
- [ ] **SQL Injection Prevention** — Use parameterized queries (EF Core does this)
- [ ] **CORS** — Restrict to trusted domains
- [ ] **HTTPS** — Encrypt all traffic
- [ ] **Logging** — Audit trails for compliance
---
 
## 📚 What This Project Demonstrates
 
### Backend Development Skills
 
✅ **Database Design**
- Multiple related entities
- Foreign keys & relationships
- Indexes for performance
✅ **API Design**
- RESTful conventions
- Proper HTTP methods & status codes
- Consistent response formats
✅ **Business Logic**
- Inventory management (prevent overselling)
- Order processing workflow
- Payment handling
✅ **Code Quality**
- SOLID principles
- Clean code practices
- Comprehensive error handling
- XML documentation comments
✅ **Advanced .NET Patterns**
- Entity Framework Core ORM
- Dependency Injection
- Async/await programming
- LINQ queries
---
 
## 🚀 Future Enhancements
 
### High Priority
- [ ] User authentication (JWT)
- [ ] Role-based authorization
- [ ] Unit tests (xUnit)
- [ ] Integration tests
- [ ] Real payment gateway integration (Stripe, PayPal)
### Medium Priority
- [ ] API versioning
- [ ] Rate limiting
- [ ] Caching (Redis)
- [ ] API key authentication
- [ ] Webhook support for order updates
### Low Priority
- [ ] GraphQL endpoint
- [ ] Message queue (RabbitMQ) for async processing
- [ ] Docker containerization
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Kubernetes deployment
---
 
## 📖 Learning Resources
 
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [REST API Best Practices](https://restfulapi.net/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
---
 
## 🤝 Interview Talking Points
 
**"This project demonstrates:"**
 
1. **Database Design** — Multiple related entities with proper indexes
2. **Complex Business Logic** — Inventory management, order workflow, payment processing
3. **API Design** — RESTful conventions, proper status codes, error handling
4. **Clean Code** — SOLID principles, separation of concerns, extensive documentation
5. **Advanced .NET** — EF Core, async/await, dependency injection, LINQ
6. **Real-World Thinking** — Stock validation, price snapshots, order cancellation flows
**Real-world scenario to explain:**
> "When creating an order, the system retrieves the customer's cart, validates stock for all items, creates the order with price snapshots (in case product prices change later), reduces inventory, and clears the cart. If stock runs out before payment, the customer can't proceed—this prevents overselling."
 
---
 
## 📝 License
 
Open source for portfolio purposes.
 
---
 
## 👤 Author
 
**Yathavi Karunakaram**  
Backend Developer | .NET | SQL  
 
[GitHub](https://github.com/yathaviR) | [LinkedIn](https://www.linkedin.com/in/yathavi--ramesh)
 
---
 
## ✅ Deployment Checklist
 
- [ ] All tests pass
- [ ] Code reviewed
- [ ] Documentation complete
- [ ] Database migrations tested
- [ ] Error handling comprehensive
- [ ] Logging implemented
- [ ] Performance optimized (indexes, queries)
- [ ] Security review done
- [ ] API documentation (Swagger) complete
- [ ] Deployed to production
---
 
**Built with ❤️ using .NET 9**
 

