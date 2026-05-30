# 🖤 VELORA — Premium Fashion & Lifestyle Store

A complete, production-grade ASP.NET Core MVC ecommerce application for a premium fashion brand. Built as a university Web Engineering project with real-world architecture and professional UI.

---

## 🌟 Features

### User Side
- 🏠 Luxury homepage with hero carousel, featured products, new arrivals, best sellers
- 🛍️ Shop with advanced filtering (category, gender, brand, price range), sorting & pagination
- 🔍 Search autocomplete with live suggestions
- 📦 Product detail pages with image gallery, size/color picker, reviews & ratings
- 🛒 Shopping cart with session persistence, quantity update, real-time totals
- 💳 Checkout with shipping info, multiple payment methods (COD, JazzCash, EasyPaisa, Card)
- 💖 Wishlist system
- 👤 User dashboard: orders, profile, wishlist, password change

### Admin Panel (`/Admin`)
- 📊 Dashboard with revenue charts, stat cards, recent orders
- 🏷️ Full CRUD for Products with image upload
- 📂 Category management
- 📦 Order management with status updates
- 👥 Customer listing
- 📧 Automated emails on order confirmation & shipping

### Technical
- ✅ ASP.NET Core 8 MVC
- ✅ Entity Framework Core Code-First with SQL Server
- ✅ ASP.NET Identity with Role-Based Authorization
- ✅ Generic + Specific Repository Pattern
- ✅ Unit of Work Pattern
- ✅ Service Layer (Cart, Order, Email, Image)
- ✅ Soft Delete for Products and Users
- ✅ MailKit SMTP email with branded HTML templates
- ✅ Global Exception Handling middleware
- ✅ Anti-forgery tokens on all forms
- ✅ Responsive Bootstrap 5 UI with custom CSS

---

## 🚀 Quick Setup

### Prerequisites
- Visual Studio 2022
- .NET 8 SDK
- SQL Server (LocalDB works fine)

### Steps

**1. Clone & Open**
```bash
git clone https://github.com/yourusername/velora.git
cd velora
```
Open `Velora.sln` in Visual Studio 2022.

**2. Configure Database**

Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VeloraDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

**3. Run Migrations**

In Package Manager Console:
```
Add-Migration InitialCreate
Update-Database
```

The app will auto-seed: 5 categories, 20+ products, admin account, banners.

**4. Run the Application**
```
Ctrl + F5
```

**5. Access Admin Panel**
```
URL:      https://localhost:xxxx/Admin
Email:    admin@velora.com
Password: Admin@123456
```

---

## 📧 Email Setup (Optional)

In `appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": "587",
  "SenderEmail": "your-gmail@gmail.com",
  "SenderPassword": "your-app-password",
  "SenderName": "Velora"
}
```

> **Gmail tip:** Enable 2FA → Generate an App Password → use that as SenderPassword.

---

## 🗄️ Database Schema

```
ApplicationUser  ←→  Orders (1:M)
ApplicationUser  ←→  Cart   (1:1)
ApplicationUser  ←→  Wishlist (1:1)
Category         ←→  Products (1:M)
Product          ←→  OrderItems, CartItems, WishlistItems, Reviews (1:M)
Order            ←→  OrderItems (1:M)
```

---

## 📁 Project Structure

```
Velora/
├── Areas/Admin/          → Admin panel controllers & views
├── Controllers/          → Home, Shop, Cart, Checkout, Account, User, Error
├── Data/                 → ApplicationDbContext, DbInitializer (seed)
├── Helpers/              → Middleware, SlugHelper, CurrencyHelper
├── Interfaces/           → IGenericRepository, IUnitOfWork, IProductRepository...
├── Models/               → ApplicationUser, Product, Category, Order, Cart...
├── Repositories/         → GenericRepository, ProductRepository, UnitOfWork...
├── Services/             → EmailService, CartService, OrderService, ImageService
├── ViewModels/           → RegisterVM, LoginVM, ShopVM, CheckoutVM, AdminDashboardVM...
├── Views/                → All Razor views
│   ├── Shared/           → _Layout.cshtml, _ProductCard.cshtml, _UserSidebar.cshtml
│   ├── Home/             → Index, About, Contact, FAQ
│   ├── Shop/             → Index, Details
│   ├── Cart/             → Index
│   ├── Checkout/         → Index, Confirmation
│   ├── Account/          → Login, Register, ForgotPassword
│   ├── User/             → Dashboard, Profile, Orders, Wishlist, ChangePassword
│   └── Error/            → NotFound, Error, Forbidden
├── wwwroot/
│   ├── css/velora.css    → Full custom stylesheet (2000+ lines)
│   └── js/velora.js      → Cart AJAX, toasts, gallery, search autocomplete
├── appsettings.json
└── Program.cs
```

---

## ☁️ Azure Deployment

**1. Create Resources**
- Azure App Service (B1 plan minimum)
- Azure SQL Database

**2. Publish from Visual Studio**
```
Right-click Project → Publish → Azure App Service → Create New
```

**3. Set Connection String in Azure Portal**
```
App Service → Configuration → Connection Strings → Add:
Name: DefaultConnection
Value: your-azure-sql-connection-string
Type: SQLAzure
```

**4. Set Environment**
```
App Service → Configuration → Application Settings:
ASPNETCORE_ENVIRONMENT = Production
```

**5. Run Migrations on Azure**
The app auto-runs `database.MigrateAsync()` on startup — migrations apply automatically.

---

## 🔐 Default Accounts

| Role     | Email               | Password      |
|----------|---------------------|---------------|
| Admin    | admin@velora.com    | Admin@123456  |

Register any new user → automatically gets "Customer" role.

---

## 🛠️ NuGet Packages

| Package | Version |
|---------|---------|
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.0 |
| Microsoft.AspNetCore.Identity.UI | 8.0.0 |
| MailKit | 4.3.0 |
| MimeKit | 4.3.0 |
| SixLabors.ImageSharp | 3.1.3 |
| Newtonsoft.Json | 13.0.3 |

---

## 📸 Color Theme

| Variable | Value |
|----------|-------|
| `--gold` | #C9A84C |
| `--black` | #0D0D0D |
| `--white` | #FFFFFF |
| `--gray-100` | #F5F5F5 |

---

## 🎓 University Project Notes

This project demonstrates:
- Clean Architecture (Repository + UoW + Service Layer)
- Role-Based + Policy-Based Authorization
- EF Core Code First with Migrations & Seed
- SMTP Email Integration
- Soft Delete pattern
- Responsive mobile-first UI
- Professional Admin Dashboard with Chart.js

---

*Built with ❤️ using ASP.NET Core 8, EF Core, Bootstrap 5*
