## Project Overview

This is a full-stack e-commerce application built with:

* **Backend:** ASP.NET Core 9.0 Web API (C#)
* **Frontend:** Angular 19
* **Database:** SQL Server (via Entity Framework Core)
* **Payments:** Stripe integration
* **Authentication:** JWT (JSON Web Tokens)
* **Image Processing:** SixLabors.ImageSharp

Key features include user registration/login, product catalog, shopping cart, checkout, order management, admin dashboard, and image upload for products.

---

## Tech Stack

* **.NET 9.0**
* **ASP.NET Core Web API**
* **Entity Framework Core (SQL Server)**
* **Angular 19** (TypeScript, RxJS)
* **Stripe.net**
* **Swagger / Swashbuckle**
* **BCrypt.Net-Next** (password hashing)
* **SixLabors.ImageSharp** (image resizing)

---

## Architecture

```
┌──────────────┐      HTTP      ┌──────────────┐
│  Angular App │ ──────────── ▶ │  ASP.NET API │
│ (frontend)   │ ◀───────────   │ (backend)    │
└──────────────┘                └──────────────┘
        │                              │
        │                              │
        ▼                              ▼
   Browser                        SQL Server
 (localhost:4200)                (EcommerceDb)
```

* **Frontend** communicates with **Backend** over REST (`/api/...`), using JWT in `Authorization` header.
* **Backend** uses **EF Core** migrations to manage database schema and **Stripe** SDK for payment sessions & webhooks.

---

## Prerequisites

* [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
* [Node.js & npm](https://nodejs.org/)
* [Angular CLI](https://angular.io/cli)
* SQL Server (LocalDB or full instance)
* (Optional) Docker & Docker Compose

---

## Installation

### 1. Clone the repository

```bash
git clone https://github.com/your-username/C--Angular-SQL-E-commerce.git
cd C--Angular-SQL-E-commerce
```

### 2. Backend Setup

1. **Configure connection string** in `server/appsettings.json` (default uses LocalDB):

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceDb;Trusted_Connection=True;"
   }
   ```

2. **Run EF Core migrations**:

   ```bash
    cd server
    dotnet ef database update
   ```


````

3. **Run the API**:

   ```bash
dotnet run
# listens on https://localhost:5292 by default
````

4. **Swagger UI** available at `https://localhost:5292/swagger`.

### 3. Database

* All schema changes are in `server/Migrations`.
* Tables include: `Users`, `Products`, `Carts` & `CartItems`, `Orders` & `OrderItems`, plus columns for roles, statuses, Stripe session IDs, images, etc.

### 4. Frontend Setup

1. **Configure API URL** in `front/src/environments/environment.ts`:

   ```ts
   export const environment = {
     production: false,
     apiUrl: 'http://localhost:5292/api',
     stripePublishableKey: 'your-pk_test_…'
   };
   ```

2. **Install dependencies & run**:

   ```bash
   cd front
   ```

npm install
npm start      # opens at [http://localhost:4200](http://localhost:4200)

```

---

## Project Structure

```

C--Angular-SQL-E-commerce/
├── C--Angular-SQL-E-commerce.sln
├── front/                    # Angular client
│   ├── src/
│   │   ├── app/
│   │   │   ├── guards/       # Admin/auth guards
│   │   │   ├── interceptors/ # JWT interceptor
│   │   │   ├── models/       # TS interfaces
│   │   │   ├── pages/        # Components by route
│   │   │   ├── services/     # HTTP services
│   │   │   ├── app.routes.ts
│   │   │   └── …
│   │   └── environments/
│   └── package.json
├── server/                   # ASP.NET Core Web API
│   ├── Controllers/          # REST endpoints
│   ├── Data/                 # AppDbContext
│   ├── Dtos/                 # Request/response DTOs
│   ├── Migrations/           # EF Core migrations
│   ├── Models/               # EF entity classes
│   ├── Middleware/           # Error handling
│   ├── Infrastructure/       # Services (e.g. Stripe)
│   ├── appsettings.json
│   ├── Program.cs
│   └── server.csproj
└── README.md

````

---

## Backend

### Controllers

- **AuthController** – Register, login (returns JWT).  
- **ProductsController** – CRUD for products (admin only where applicable).  
- **CartController** – Add/remove items, view cart.  
- **OrdersController** – Place order, get order history/status.  
- **UsersController** – Get/update profile.  
- **StripeWebhookController** – Handle Stripe events (payment succeeded, etc.).

### Models & DTOs

- **Models**: `User`, `Product`, `Cart`, `CartItem`, `Order`, `OrderItem`, plus settings (`JwtSettings`, `StripeSettings`).  
- **DTOs** map between HTTP payloads and domain entities (e.g. `CreateCheckoutSessionDto`, `UserDto`, `UpdateProfileDto`).

### Database Context & Migrations

- **AppDbContext** configures EF Core entities & relationships.  
- Schema evolves through migrations in `server/Migrations` (InitialCreate → AddUser → Cart → Order → Status → Stripe fields → Images).  

### Middleware

- **ErrorHandlingMiddleware** catches exceptions and returns standardized error responses.

### Security

- **JWT Authentication** via `Microsoft.AspNetCore.Authentication.JwtBearer`.  
- **BCrypt** for password hashing.

### Stripe Integration

- **Stripe.net** SDK for creating checkout sessions.  
- **Webhook** endpoint verifies event signature and updates order status/payment intent.

### Swagger

- Auto-generated API docs via Swashbuckle at `/swagger`.

---

## Frontend

### Services

- **AuthService** – Login, register, token/storage.  
- **ProductService** – Fetch products, product details, images.  
- **CartService** – Manage cart items in session & via API.  
- **ProfileService**, **OrderService**, **UserService** – Profile & order history.

### Models

- TS interfaces for `User`, `Product`, `CartItem`, `Order`, etc.

### Routing

Defined in `app.routes.ts`, mapping paths like `/home`, `/products`, `/product/:id`, `/cart`, `/checkout`, `/orders`, `/admin`, etc.

### Guards & Interceptors

- **AuthInterceptor** injects JWT into HTTP headers.  
- **AuthGuard** & **AdminGuard** protect routes by role.

### Pages & Components

Folder structure under `src/app/pages`:

- **home**, **products**, **product-detail**, **cart**, **checkout**, **payment-success**/**payment-cancel**, **orders**, **order-detail**, **profile**, **register**, **login**, **admin** (add/edit products), **terms**, **privacy**, **cookies**.

---

## Environment Variables

| Name                                  | Purpose                                             |
| ------------------------------------- | --------------------------------------------------- |
| `ConnectionStrings__DefaultConnection`| SQL Server connection (e.g. LocalDB)                |
| `JwtSettings__Key`                    | Secret key for signing JWTs                        |
| `JwtSettings__Issuer`                 | JWT issuer                                          |
| `JwtSettings__Audience`               | JWT audience                                        |
| `JwtSettings__ExpireMinutes`          | Token expiry (minutes)                              |
| `Stripe__SecretKey`                   | Stripe secret key (server)                          |
| `Stripe__PublishableKey`              | Stripe publishable key (client)                     |
| `Stripe__WebhookSecret`               | Stripe webhook signing secret                       |
| `environment.apiUrl` (frontend)       | Base URL for API calls (e.g. `http://localhost:5292/api`) |
| `environment.stripePublishableKey`    | Stripe publishable key for checkout session creation |

---

## Running with Docker (optional)

```yaml
# docker-compose.yml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "Your_strong!Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
  api:
    build: ./server
    environment:
      ConnectionStrings__DefaultConnection: "Server=db;Database=EcommerceDb;User Id=sa;Password=Your_strong!Passw0rd;"
      …
    ports:
      - "5292:80"
    depends_on:
      - db
  front:
    build: ./front
    ports:
      - "4200:80"
    depends_on:
      - api
````

```bash
docker-compose up --build
```

---

## Testing

* **Backend unit tests** (if any) with `dotnet test` in a `server.Tests` project.
* **Frontend tests** via `npm test`.

---

## Contributing

1. Fork the repo
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m "Add YourFeature"`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

---

## License

This project is licensed under the [MIT License](LICENSE).
