<div align="center">
<h1> Hotel Module</h1>
</div>

## Overview 
This project is a comprehensive hotel booking platform designed to provide users with a seamless experience in searching for hotels, viewing detailed information, and making secure bookings. The platform features an advanced search capability, personalized recommendations, and a secure checkout process. For administrators, there is a dedicated interface for managing cities, hotels, and rooms, with functionalities for creating, updating, and deleting entries. User authentication is handled securely through JWT, ensuring robust role-based access control and a maintainable system architecture.

<a href="https://whimsical.com/function-PgTQUTGPFg5SWjqSueYh7x@GjdVyJ6vu7AX2orrZkLYPgQZBb99MoadQxTwLgPFdA24">Functional Requirements And Non-Functional Requirements</a>

## 🗂 Database Schema Design
<img width="935" alt="DataBaseDigram" src="https://github.com/user-attachments/assets/a79cfc43-dfc4-47ec-9f01-68f9a92606ac">

## Key Features 🌟
### 1.🔒 User Authentication
- Secure login page with JWT-based authentication.
- Role-based access control for different user permissions.
### 2.🔍 Advanced Search Functionality
- Central search bar for finding hotels and cities.
- Interactive calendar for selecting check-in and check-out dates.
- Adjustable controls for specifying the number of adults, children, and rooms.
### 3.📈 Personalized Recommendations
- Featured deals highlighting special offers on selected hotels.
- Display of recently visited hotels based on user history.
- Trending destinations showcasing the most popular cities.
### 4.🤝 Comprehensive Search Filters
- Filters for price range, star ratings, amenities, and room types.
- Infinite scroll for browsing hotel listings.
### 5.🏠 Detailed Hotel Pages
- High-quality image gallery with fullscreen viewing.
- Detailed information about the hotel, including reviews and an interactive map.
- Room availability and selection with the option to add rooms to the cart for booking.
### 6.✅ Secure Checkout Process
- Collection of user information and payment details.
- Booking confirmation with details, printable options, and email notifications.
### 7.⚙️ Admin Management Interface
- Easy navigation and management of cities, hotels, and rooms.
- Search filters and detailed grids for administrative tasks.
- Forms for creating and updating cities, hotels, and rooms.
### 8.🔧 API Design and Management
- RESTful APIs for all functionalities with clear documentation.
- Robust error handling and logging for debugging and monitoring.
### 9.🛠️ Performance and Scalability
- Efficient data handling for optimized performance.
- Scalable architecture to support growing user and data demands.
### 10.🎯 Comprehensive Testing
- Unit testing, integration testing, and API testing to ensure reliability and performance.
- CI/CD pipeline for automated testing and deployment.
  
## 🌐 API Endpoints

### User Endpoints

| HTTP Method | Endpoint             | Description                                                                                 | Authorization |
|-------------|----------------------|---------------------------------------------------------------------------------------------|---------------|
| POST        | /api/auth/register   | Create a new user account with username, password, and email.                             | All         |
| POST        | /api/auth/login      | Authenticate a user and generate a token if credentials are valid.                         | All          |

### Role Endpoints

| HTTP Method | Endpoint          | Description                                             | Authorization |
|-------------|-------------------|---------------------------------------------------------|---------------|
| POST        | /api/auth/assign-role | Assign a role to a user identified by their email address. | Admin         |

### Token Endpoints

| HTTP Method | Endpoint         | Description                                               | Authorization |
|-------------|------------------|-----------------------------------------------------------|---------------|
| POST        | /api/token/revokeToken | Revokes the specified token and removes it from the user's refresh tokens. | User or Admin  |

#### Detailed Endpoint Description:

- **POST /api/token/revokeToken**
  - **Description**: Revokes the specified token and removes it from the user's refresh tokens.
  - **Request Body**: 
    ```json
    {
      "token": "string"  // The token to be revoked
    }
    ```
  - **Responses**:
    - **200 OK**: If the token was successfully revoked.
    - **400 Bad Request**: If the token is invalid or could not be revoked.
  - **Authorization**: Requires user or admin authentication.
    
### Owner Endpoints

| HTTP Method | Endpoint              | Description                                       | Authorization |
|-------------|-----------------------|---------------------------------------------------|---------------|
| GET         | /api/owner/{id}        | Retrieve an owner by its unique identifier.      | All           |
| POST        | /api/owner             | Create a new owner.                             | Admin         |
| PUT         | /api/owner/{id}        | Update an existing owner.                       | Admin         |
| DELETE      | /api/owner/{id}        | Delete an existing owner.                       | Admin         |
| GET         | /api/owner             | Retrieve all owners.                            | All           |

### City Endpoints

| HTTP Method | Endpoint                                      | Description                                                       | Authorization |
|-------------|-----------------------------------------------|-------------------------------------------------------------------|---------------|
| POST        | /api/city                                     | Add a new city.                                                   | Admin         |
| GET         | /api/city                                     | Retrieve cities with optional filtering by name and description.  | All           |
| GET         | /api/city/{id}                                | Retrieve a city by its unique identifier, with optional hotels.   | All          |
| PUT         | /api/city/{id}                                | Update the information of an existing city.                       | Admin         |
| DELETE      | /api/city/{id}                                | Delete an existing city.                                          | Admin         |
| GET         | /api/city/{cityId}/hotels                     | Retrieve hotels for a specific city.                              | All          |
| POST        | /api/city/{cityId}/hotels                     | Add a hotel to a specific city.                                   | Admin         |
| DELETE      | /api/city/{cityId}/hotel/{hotelId}            | Remove a hotel from a specific city.                              | Admin         |
| POST        | /api/city/{cityId}/upload-image               | Upload an image for a specific city.                              | Admin         |
| DELETE      | /api/city/{cityId}/delete-image/{publicId}    | Delete an image from a specific city.                             | Admin         |
| GET         | /api/city/{cityId}/images                     | Get all images for a specific city.                               | All          |

### Hotel Endpoints

| HTTP Method | Endpoint                                  | Description                                                     | Authorization |
|-------------|-------------------------------------------|-----------------------------------------------------------------|---------------|
| POST        | /api/hotel                                | Add a new hotel.                                                | Admin         |
| GET         | /api/hotel                                | Retrieve hotels with optional filtering by name and description.| All           |
| GET         | /api/hotel/{id}                           | Retrieve a hotel by its unique identifier.                      | All           |
| PUT         | /api/hotel/{id}                           | Update the information of an existing hotel.                    | Admin         |
| DELETE      | /api/hotel/{id}                           | Delete an existing hotel.                                       | Admin         |
| GET         | /api/hotel/{hotelId}/rooms                | Retrieve rooms for a specific hotel.                            | All           |
| POST        | /api/hotel/{hotelId}/rooms                | Add a room to a specific hotel.                                 | Admin         |
| DELETE      | /api/hotel/{hotelId}/room/{roomId}        | Remove a room from a specific hotel.                            | Admin         |
| POST        | /api/hotel/{hotelId}/upload-image         | Upload an image for a specific hotel.                           | Admin         |
| DELETE      | /api/hotel/{hotelId}/delete-image/{publicId}| Delete an image from a specific hotel.                         | Admin         |
| GET         | /api/hotel/{hotelId}/images               | Get all images for a specific hotel.                            | All           |
| POST        | /api/hotel/{hotelId}/amenities            | Add an amenity to a specific hotel.                             | Admin         |
| DELETE      | /api/hotel/{hotelId}/amenities/{amenityId}| Remove an amenity from a specific hotel.                        | Admin         |
| GET         | /api/hotel/{hotelId}/amenities            | Get all amenities for a specific hotel.                         | All           |
| GET         | /api/hotel/{id}/rating                    | Get the review rating of a specific hotel.                      | All           |

### Amenity Endpoints

| HTTP Method | Endpoint                         | Description                                                                                                               | Authorization |
|-------------|----------------------------------|---------------------------------------------------------------------------------------------------------------------------|---------------|
| GET         | /search-results/amenities         | Retrieve all available amenities. Includes details like name, description, and associated room classes.                  | All           |

### Booking Endpoints

| HTTP Method | Endpoint                         | Description                                                                                                               | Authorization |
|-------------|----------------------------------|---------------------------------------------------------------------------------------------------------------------------|---------------|
| POST        | /api/booking/confirm              | Confirm a booking and send a confirmation email. Requires booking confirmation data.                                     | All           |
| GET         | /api/booking/{id}                 | Retrieve a booking by its unique identifier.                                                                             | Admin         |
| POST        | /api/booking/create              | Create a new booking. Requires user email in the token for identification.                                                | All           |
| PUT         | /api/booking/{id}/Update_status  | Update the status of a booking. Requires user email in the token.                                                         | User          |

### Home Page Endpoints

| HTTP Method | Endpoint                 | Description                                                                                                         | Authorization |
|-------------|--------------------------|---------------------------------------------------------------------------------------------------------------------|---------------|
| GET         | /api/homepage/trending-destinations | Get the top 5 most visited cities with details.                                                                  | All           |
| GET         | /api/homepage/search      | Search for hotels based on various criteria such as city name, star rating, number of rooms, and dates.           | All           |

### Invoice Endpoints

| HTTP Method | Endpoint                    | Description                                                             | Authorization |
|-------------|-----------------------------|-------------------------------------------------------------------------|---------------|
| POST        | /api/invoice                | Creates a new invoice record.                                           | Admin         |
| GET         | /api/invoice/{id}           | Retrieves a specific invoice record by ID.                              | All           |
| GET         | /api/invoice/by-booking/{bookingId} | Retrieves all invoice records for a specific booking.                  | All           |
| PUT         | /api/invoice/{id}           | Updates an existing invoice record.                                     | Admin         |
| DELETE      | /api/invoice/{id}           | Deletes an invoice record.                                              | Admin         |

### Review Endpoints

| HTTP Method | Endpoint               | Description                                                          | Authorization |
|-------------|------------------------|----------------------------------------------------------------------|---------------|
| POST        | /api/review            | Creates a new review. Requires 'User' role.                         | User          |
| GET         | /api/review/{id}       | Retrieves a review by its ID.                                        | All           |
| PUT         | /api/review/{id}       | Updates an existing review. Requires 'User' role.                    | User          |
| DELETE      | /api/review/{id}       | Deletes a review by its ID. Requires 'Admin' role.                   | Admin         |

### Room Class Endpoints

| HTTP Method | Endpoint                                    | Description                                                          | Authorization |
|-------------|---------------------------------------------|----------------------------------------------------------------------|---------------|
| POST        | /api/roomclass                              | Creates a new room class. Requires 'Admin' role.                     | Admin         |
| GET         | /api/roomclass/{id}                         | Retrieves a specific room class by ID.                               | All           |
| PUT         | /api/roomclass/{id}                         | Updates an existing room class. Requires 'Admin' role.                | Admin         |
| POST        | /api/roomclass/{roomClassId}/addamenity     | Adds an amenity to a specific room class. Requires 'Admin' role.      | Admin         |
| DELETE      | /api/roomclass/{roomClassId}/amenities/{amenityId} | Deletes an amenity from a specific room class. Requires 'Admin' role. | Admin         |
| GET         | /api/roomclass/{roomClassId}/amenities      | Retrieves all amenities for a specific room class.                    | All           |
| POST        | /api/roomclass/{roomClassId}/rooms          | Adds a new room to a specific room class. Requires 'Admin' role.      | Admin         |
| GET         | /api/roomclass/{roomClassId}/rooms          | Retrieves all rooms for a specific room class.                        | All           |
| DELETE      | /api/roomclass/{roomClassId}/rooms/{roomId} | Deletes a specific room from a room class. Requires 'Admin' role.     | Admin         |
| POST        | /api/roomclass/{roomClassId}/upload-image   | Uploads an image for a specific room class. Requires 'Admin' role.    | Admin         |
| DELETE      | /api/roomclass/{roomClassId}/delete-image/{publicId} | Deletes an image from a specific room class. Requires 'Admin' role.   | Admin         |
| GET         | /api/roomclass/{roomClassId}/image/{publicId} | Retrieves details of an image associated with a specific room class. | All           |

### Room Endpoints

| HTTP Method | Endpoint                                        | Description                                                      | Authorization |
|-------------|-------------------------------------------------|------------------------------------------------------------------|---------------|
| GET         | /api/room/by-price                              | Retrieves rooms within a specific price range.                   | All           |
| GET         | /api/room/{id}                                 | Retrieves a specific room by ID.                                | All          |
| POST        | /api/room/{roomId}/upload-image                 | Uploads an image for a specific room. Requires 'Admin' role.     | Admin         |
| DELETE      | /api/room/{roomId}/delete-image/{publicId}      | Deletes an image from a specific room. Requires 'Admin' role.    | Admin         |
| GET         | /api/room/{roomId}/image/{publicId}             | Retrieves details of an image associated with a specific room.  | All         |
| GET         | /api/room/available-without-bookings            | Retrieves available rooms with no bookings.                      | All          |

### Discount Endpoints

| HTTP Method | Endpoint                     | Description                                                                                                 | Authorization |
|-------------|------------------------------|-------------------------------------------------------------------------------------------------------------|---------------|
| POST        | /api/discount                | Adds a new discount to a room.                                                                             |Admin          |
| PATCH       | /api/discount/{id}           | Updates a discount by ID.                                                                                   | Admin         |
| GET         | /api/discount                | Retrieves a list of all available discounts.                                                               | All          |
| GET         | /api/discount/{id}           | Retrieves the details of a specific discount by ID.                                                         | All         |
| DELETE      | /api/discount/{id}           | Deletes a discount by ID.                                                                                   | Admin          |
| GET         | /api/discount/active         | Retrieves a list of currently active discounts.                                                            | All          |
| GET         | /api/discount/top-discounts   | Retrieves a list of rooms with the highest active discounts.                                                | All         |

### Image Endpoints

| HTTP Method | Endpoint                                 | Description                                                                                         | Authorization |
|-------------|------------------------------------------|-----------------------------------------------------------------------------------------------------|---------------|
| POST        | /api/image/{entityType}/{entityId}/upload-image | Uploads an image for a specific entity (e.g., room, hotel) using the entity type and ID.           | Admin         |
| GET         | /api/image/images/{type}                  | Retrieves all images associated with a specific entity type.                                        | User or Admin  |
| DELETE      | /api/image/delete-image/{publicId}        | Deletes an image by its PublicId.                                                                    | Admin         |
| GET         | /api/image/details/{publicId}             | Retrieves details of an image by its PublicId.                                                       | User or Admin  |

### Hotel Amenities Endpoints

| HTTP Method | Endpoint                      | Description                                        | Authorization     |
|-------------|-------------------------------|----------------------------------------------------|-------------------|
| GET         | /api/hotelAmenities            | Retrieve amenities by hotel name with optional pagination. | Admin, User       |
| GET         | /api/hotelAmenities/all        | Retrieve all amenities by hotel name.             | Admin, User       |
| PUT         | /api/hotelAmenities/{amenityId} | Update a specific amenity by its ID.              | Admin             |

## Layered architecture
`Clean Architecture`
It achieves this by separating the application into different layers that have distinct responsibilities:

<img width="253" alt="clean-architecture-layers" src="https://github.com/user-attachments/assets/d22c667b-6f18-4728-a6f0-b6c514e16e06">

### 1. Domain Layer
Contains the core business logic and domain entities. This layer is independent of other layers and focuses on the business rules and domain models.
#### Components:
- Entities: Represent the core data models. For example, User, Role, Hotel, Room, Booking.
- Value Objects: Define immutable objects used within the domain. For example, Address, Money.
- DTOs (Data Transfer Objects): Define the data structures used for communication between the application layer and the API layer. For example, UserDto, BookingDto.
- Domain Repository: Implement business logic that does not naturally fit within an entity. For example, DiscountRepository.
### 2. Infrastructure Layer
Provides implementations for data access, external services, and other system-level concerns. This layer interacts with databases, file systems, and external APIs.
#### Components:
- Repositories: Implement data access logic and interact with the database. For example, UserRepository, HotelRepository.
- Data Context: Manages database connections and operations. For example, AppDbContext.
- Configurations: Manage configurations for external services and data access.
### 3. Application Layer
Orchestrates the application's workflow, coordinates interactions between domain models and infrastructure, and handles application-specific logic.
#### Components:
- Services: Implement application-specific logic and use domain services and repositories to perform operations. For example, UserService, BookingService.
- Application Interfaces: Define contracts for services and repositories. For example, IUserService, IBookingService.
 - External Services: Handle interactions with third-party services. For example, image storage services, payment gateways.
### 4. API Layer
Exposes endpoints for client interaction and handles HTTP requests and responses. This layer is responsible for routing, validation, and transforming data between the application layer and clients.
#### Components:
- Controllers: Handle incoming HTTP requests, invoke application services, and return responses. For example, UserController, BookingController.
- Middlewares: Implement cross-cutting concerns such as authentication, authorization, and logging.
- Response Handlers: Standardize API responses and error handling. For example, ResponseHandler.

## Image Storage

### 1. Image Upload
- Endpoint: `POST /api/image/upload`

### 2. Image Retrieval
- Endpoint:` GET /api/image/details/{publicId}`

### 3. Image Deletion
- Endpoint: `DELETE /api/image/delete-image/{publicId}`
## 📩  Booking Confirmation 
Once a booking is confirmed, users receive an email containing the details of their reservation. Below is an example of the booking confirmation email that users receive:

![0](https://github.com/user-attachments/assets/14aff3a6-15e8-4572-a782-840c251c0e90)

## 🔋🪫 Testing Frameworks 
Ensuring code quality and functionality with comprehensive testing suites using:

- ##### xUnit: A popular .NET testing framework that facilitates writing and executing unit tests with a focus on simplicity and extensibility. ✅
- ##### Moq: A powerful mocking library for .NET that allows creating mock objects for unit testing, enabling isolated and controlled test scenarios. 🧪
- ##### FluentAssertions: A library that provides a more expressive and readable syntax for assertions, making tests easier to write and understand. 📏
- ##### Fixture: A testing concept used to set up shared contexts for multiple test cases, allowing for efficient and consistent test execution. 🧩

## Design Patterns
In the Hotel Booking Platform, several design patterns are employed to ensure a clean, maintainable, and scalable architecture. Here’s a summary of the key patterns used:
### 1. Unit of Work Pattern
The Unit of Work pattern is used to manage changes to multiple entities in a single transaction. This pattern ensures that changes to the database are handled in a single unit, maintaining data consistency and integrity.

<img width="457" alt="Unitofwork" src="https://github.com/user-attachments/assets/a4007b80-a962-4fca-952e-d2bee8655f3c">

### 2. Generic Repository Pattern
The Generic Repository pattern provides a way to manage CRUD operations in a consistent and reusable manner. This pattern abstracts the data access layer and provides a generic implementation for common operations.

![layers](https://github.com/user-attachments/assets/2eded6b6-715e-415b-b7a3-63a4d17db03c)

## Setup Guide

#### 1. **System Requirements**

- **Operating System**: Windows 10 or higher, macOS, or Linux
- **Software**: .NET Core SDK 6.0 or later, Node.js (if applicable), Docker (if using containers)
- **Hardware**: Minimum 4 GB RAM, 2 CPU cores

#### 2. **Clone the Repository**

```bash
git clone https://github.com/LePointManagementSystem/hotel-module.git
cd hotel-module
```

#### 3. **Cloudinary Integration**

Cloudinary is used to manage and store images efficiently. The `appsettings.json` contains the necessary configurations:
```json
"CloudinarySettings": {
  "CloudName": "",
  "ApiKey": "",
  "ApiSecret": ""
}
````

#### 4. **Run Migrations**

```bash
dotnet ef migrations add <MigrationName> --project HotelBookingPlatform.Infrastructure --startup-project HotelBookingPlatform.API
dotnet ef database update --project HotelBookingPlatform.Infrastructure --startup-project HotelBookingPlatform.API
```

#### 5. **Start Database and PgAdmin**

```bash
docker-compose up -d
```
Open your browser and navigate to: http://localhost:8888/

## Docker Deployment On EC2

### Files added for deployment

- `Dockerfile`: builds and publishes the ASP.NET Web API into a container image.
- `docker-compose.deploy.yml`: runs the API for EC2-style deployment with Neon as the production database.
- `.env.deploy.example`: template for production environment variables.
- `scripts/generate-self-signed-cert.ps1`: creates a `.pfx` self-signed certificate on Windows.
- `scripts/generate-self-signed-cert.sh`: creates a `.pfx` self-signed certificate on Linux/macOS.
- `scripts/docker-entrypoint.sh`: generates the certificate inside the container on first boot if it is missing.

### 1. Create the self-signed certificate

On Windows:

```powershell
.\scripts\generate-self-signed-cert.ps1 -DnsName "your-ec2-public-dns" -Password "change-this-pfx-password"
```

On Linux/macOS:

```bash
chmod +x ./scripts/generate-self-signed-cert.sh
./scripts/generate-self-signed-cert.sh your-ec2-public-dns change-this-pfx-password
```

This creates:

- `certs/hotelbooking-selfsigned.pfx`

The generated certificate includes a `subjectAltName` entry for the DNS name, which helps modern browsers accept the hostname correctly.

You can also skip manual certificate generation. The container now creates `/https/hotelbooking-selfsigned.pfx` automatically on first startup if the file is missing and `CERT_PASSWORD` is set.

### 2. Prepare environment variables

Copy the deployment template and fill in real values:

```bash
cp .env.deploy.example .env.deploy
```

Important values:

- `NEON_CONNECTION_STRING`
- `CERT_PASSWORD`
- `CERT_DNS_NAME`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `JWT_KEY`
- `FRONTEND_ORIGINS`

Example EC2 + Neon production values:

```env
NEON_CONNECTION_STRING=Host=ep-quiet-sky-123456.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=<YOUR_NEON_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true

CERT_PASSWORD=<A_STRONG_PFX_PASSWORD>
CERT_DNS_NAME=ec2-3-91-120-10.compute-1.amazonaws.com
CERT_VALID_DAYS=365
APPLY_MIGRATIONS_ON_STARTUP=true

JWT_ISSUER=https://ec2-3-91-120-10.compute-1.amazonaws.com
JWT_AUDIENCE=https://ec2-3-91-120-10.compute-1.amazonaws.com
JWT_KEY=<A_LONG_RANDOM_32+_CHAR_SECRET>
FRONTEND_ORIGINS=http://localhost:5173,https://your-vercel-app.vercel.app

CLOUDINARY_CLOUD_NAME=<YOUR_CLOUDINARY_CLOUD_NAME>
CLOUDINARY_API_KEY=<YOUR_CLOUDINARY_API_KEY>
CLOUDINARY_API_SECRET=<YOUR_CLOUDINARY_API_SECRET>

EMAIL_FROM_ADDRESS=<YOUR_SMTP_EMAIL>
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USERNAME=<YOUR_SMTP_USERNAME>
EMAIL_PASSWORD=<YOUR_SMTP_PASSWORD>
EMAIL_ENABLE_SSL=true

SEED_ADMIN_EMAIL=admin@yourdomain.com
SEED_ADMIN_PASSWORD=<A_STRONG_ADMIN_PASSWORD>
SEED_ADMIN_FIRST_NAME=Hotel
SEED_ADMIN_LAST_NAME=Admin

BOOKING_CLEANUP_INTERVAL=3
```

Mapping guidance:

- `CERT_DNS_NAME`, `JWT_ISSUER`, and `JWT_AUDIENCE` should all use the same EC2 public DNS or your real domain.
- `NEON_CONNECTION_STRING` should come from the Neon dashboard connection details for your production branch/database.
- `FRONTEND_ORIGINS` should list every browser origin allowed to call the API, separated by commas.
- If you later put the API behind an ALB or Nginx with a trusted cert, set `JWT_ISSUER` and `JWT_AUDIENCE` to that public HTTPS hostname instead.

### 3. Build and run on EC2

```bash
docker compose --env-file .env.deploy -f docker-compose.deploy.yml up -d --build
```

The API will be exposed on:

- `http://<ec2-host>`
- `https://<ec2-host>`

### 4. Optional startup migrations

If `APPLY_MIGRATIONS_ON_STARTUP=true`, the API container applies EF Core migrations automatically against Neon before serving traffic.

### Notes

- Browsers will warn on a self-signed certificate. This is expected.
- The deploy compose file no longer starts PostgreSQL locally; it expects Neon through `NEON_CONNECTION_STRING`.
- For public production traffic, a trusted certificate from ACM, ALB, Nginx, or Let's Encrypt is better than self-signed TLS.

## Docker Image CI

A GitHub Actions workflow now builds the container image and pushes it to GitHub Container Registry (GHCR):

- Workflow: `.github/workflows/docker-image.yml`
- Registry target: `ghcr.io/<owner>/<repo>`

Behavior:

- Pull requests build the image without pushing.
- Pushes to `main` or `master` build and push.
- Tags like `v1.0.0` also build and push tagged images.

To use the published image on EC2:

```bash
docker pull ghcr.io/<owner>/<repo>:latest
```
