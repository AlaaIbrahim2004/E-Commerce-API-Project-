<p align="center">
  <img src="e-commerce .png" alt="Masaaq Swagger UI" width="800">
</p>
# 🛒 E-Commerce RESTful API

> **🎓 Route IT Training Center - Backend (.NET Core) Diploma Project**
> *A highly scalable and robust backend API built with a focus on clean code principles, performance, and modern software architecture.*

## 🌐 Live Demo
**🌍 Live API Base URL:** [Swagger UI (ASP.NET Monster)](http://e-commerce01.runasp.net/swagger/index.html)
*(To explore the endpoints, append `/swagger` to the base URL)*

## 📝 Project Overview
This repository contains the backend infrastructure for a fully-featured E-Commerce platform. The API is designed to handle complex product catalogs, secure user authentication, and high-speed cart operations, ensuring a seamless shopping experience. 

## 🏗️ System Architecture (Onion Architecture)
To ensure high maintainability, testability, and a strict separation of concerns, the solution is organized using **Onion Architecture** into the following decoupled layers:
* **Core Layer:** Contains the domain entities, business models, and core interfaces.
* **Infrastructure Layer:** Handles data access operations using **Entity Framework Core**, implementing the Repository Pattern and Unit of Work.
* **Service/Shared Layer:** Encapsulates business logic and utilizes **DTOs (Data Transfer Objects)** to securely map and format API responses.
* **Web (API) Layer:** The presentation layer containing controllers, routing, and dependency injection configurations.
- **Test Layer:** An isolated xUnit test project dedicated to validating core business logic and services independently.
  
## ✨ Key Features & Technical Highlights

* **🗄️ Database Segmentation (Separation of Concerns):** Enhanced security and data management by structuring the application to use **two separate databases**:
  * **Identity Database:** Dedicated exclusively to managing user credentials, roles, and security tokens.
  * **Store/App Database:** Manages core business entities including products, orders, categories, and brands.
* **🔒 Secure Authentication & Authorization:** Developed robust RESTful endpoints for user registration, login, and role management utilizing **ASP.NET Core Identity** secured with **JWT (JSON Web Tokens)**.
* **⚡ High-Speed Shopping Basket (Redis):** Integrated **Redis** as an in-memory distributed cache to manage the user's shopping cart, providing extremely fast read/write operations and high availability.
* **📦 Advanced Product Catalog Operations:** Implemented comprehensive product endpoints supporting advanced filtering, sorting, and pagination mechanisms.

## 🧪 Unit Testing & Quality Assurance
The core business logic is rigorously tested to ensure software reliability, error resilience, and ease of future refactoring:
- **Testing Frameworks:** Built using **xUnit** and **Moq** for isolating unit tests from external dependencies (Database, Redis, Mapper).
- **Advanced Mocking Techniques:** Employed **MockQueryable** to mock async `IQueryable` calls on ASP.NET Core Identity's `UserManager`.
- **Verified Code Coverage:** 
  - Achieved **71% Branch Coverage** across application services.
  - Reached **100% Coverage** on `AuthenticationService` and **82%+ Coverage** on `BasketService`.
- **Coverage Reporting:** Generated comprehensive HTML coverage metrics using **Cobertura XML** and **ReportGenerator**.

## 🚀 Deployment & Cloud Architecture
The application is fully deployed and accessible online utilizing modern cloud services:
* **Backend Hosting & SQL Databases:** The main API and both SQL Server databases (Identity & Store) are deployed and hosted on **ASP.NET Monster (RunASP)**.
* **Redis Cloud Hosting:** The Redis caching layer is deployed globally using **Upstash**, providing a serverless and highly responsive connection for cart operations.
