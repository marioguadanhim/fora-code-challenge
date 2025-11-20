# Project Setup & Usage Guide

## Prerequisites

Before running the application, make sure that **appsettings.json** contains the correct information to connect to your local SQL Server database.
By default, the project is configured to use **Windows Authentication** for SQL Server.

---

## Application Entry Points

This solution contains **two entry points**:

1. **API** (Web Application)
2. **Data Importer Worker** (Console Application)

Both applications are configured to:

* Automatically run **Entity Framework migrations** on startup
* Automatically create the **database and schema** if they do not exist

---

## Running the Data Importer

1. Navigate to the **Data Importer Worker** project located in folder `2.`
2. Run the console application
3. You should see all company data being imported and displayed in the console output

This step populates the database with the required data for the API.

---

## Running the API

After the data import is complete:

1. Start the API project
2. Access Swagger via the browser

### Authentication

The API includes a login endpoint for authentication.

Default user credentials:

* **Username:** guest
* **Password:** guest

Use these credentials to generate a JWT token.

### Using the Token in Swagger

1. Copy the generated token
2. Click the **Authorize** button in Swagger
3. In the input field, enter:

```
Bearer YOUR_GENERATED_TOKEN
```

You will now be authenticated to access the secured endpoints.

---

## Unit Tests

All unit tests are expected to:

* Run successfully
* Execute without failures

You are free to run and validate all tests at any time.

---

## Summary

1. Configure database connection in `appsettings.json`
2. Run the **Data Importer Worker**
3. Verify data import in the console
4. Run the **API**
5. Authenticate using guest/guest
6. Use the token in Swagger
7. Run unit tests ✅

---

Happy coding 🚀
