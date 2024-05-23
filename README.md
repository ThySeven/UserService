# UserService

UserService is a microservice responsible for managing user data within our Auction Core Services architecture. It handles tasks such as creating, updating, authenticating, and deleting users. The service is designed to integrate seamlessly with other essential services including Mail Service, RabbitMQ, MongoDB, and HashiCorp Vault for secure management of credentials.

## Table of Contents

- [Setup](#setup)
- [Configuration](#configuration)
- [Architecture](#architecture)
- [Dependencies](#dependencies)
- [Endpoints](#endpoints)
- [Security](#security)
- [Monitoring & Logging](#monitoring--logging)
- [Continuous Deployment](#continuous-deployment)
- [License](#license)

## Setup

### Prerequisites

- Docker
- Docker Compose
- Nginx (for reverse proxy)
- HashiCorp Vault
- RabbitMQ
- Prometheus, Loki, and Grafana for monitoring

### Local Installation

1. Clone the repository:

    ```bash
    git clone https://github.com/yourusername/UserService.git
    cd UserService
    ```

2. Run the service using Docker Compose:

    ```bash
    docker-compose up --build
    ```

### Production Setup

For a production setup, ensure that you have configured Nginx, Vault, RabbitMQ, and monitoring systems accurately. You can deploy the service using your CI/CD pipeline.

## Configuration

### Environment Variables

- `LokiEndpoint`: Endpoint for Loki.
- `RabbitMQHostName`: Hostname for the RabbitMQ server.
- `VAULT_IP`: Address of the HashiCorp Vault service.
- `VAULT_SECRET`: Secret for accessing Vault.
- `MongoDBConnectionString`: Connection string for MongoDB.
- `GrafanaHostname`: Hostname for Grafana (default is `userservice`).
- `RabbitMQQueueName`: Queue name for RabbitMQ (default is `MailQueue`).
- `PublicIP`: Public IP address (default is `http://localhost:3015`).
- `ASPNETCORE_URLS`: URL for the ASP.NET Core service (default is `http://+:3015`).

Rename the file to `.env` and fill in the necessary values.

## Architecture

UserService follows a microservice architecture within the broader Auction Core Services ecosystem. It interacts closely with the following components:

- **Mail Service**: For sending user-related emails.
- **RabbitMQ**: For sending notifications and updates.
- **MongoDB**: For data persistence.
- **Nginx**: For managing API traffic and load balancing.

[Architecture Diagram](https://s.icepanel.io/mB4kr95xX1FRKO/LEDe)

## Dependencies

- MongoDB
- RabbitMQ
- HashiCorp Vault

## Endpoints

### API Endpoints

#### Create User

- **Method**: POST
- **Endpoint**: `/user/create`
- **Description**: Creates a new user.
- **Example**: POST `/user/create`
- **Output**: User
- **JSON Request**:
    ```json
    {
      "Id": "456",
      "FirstName": "Jane",
      "LastName": "Smith",
      "Email": "jane.smith@example.com",
      "Username": "janesmith",
      "Password": "password123",
      "Address": "789 Oak St, Springfield, USA",
      "PhoneNumber": "555-654-3210",
      "Verified": true
    }
    ```

#### Login

- **Method**: POST
- **Endpoint**: `/user/login`
- **Description**: Validates user credentials for login.
- **Example**: POST `/user/login`
- **Output**: JWT Token
- **JSON Request**:
    ```json
    {
      "Username": "janesmith",
      "Password": "password123"
    }
    ```

#### Get User by ID

- **Method**: GET
- **Endpoint**: `/user/{id}`
- **Description**: Retrieves user information by their unique ID.
- **Output**: User
- **Example**: GET `/user/456`

#### Update User

- **Method**: PUT
- **Endpoint**: `/user/update`
- **Description**: Updates an existing user's information.
- **Example**: PUT `/user/update`
- **Output**: Confirmation and JWT Token
- **JSON Request**:
    ```json
    {
      "Id": "456",
      "Username": "johndoe"
    }
    ```

#### Update Password

- **Method**: PUT
- **Endpoint**: `/user/updatepassword`
- **Description**: Updates the user's password for login.
- **Example**: PUT `/user/updatepassword`
- **Output**: User
- **JSON Request**:
    ```json
    {
      "LoginModel": {
        "Username": "johndoe",
        "Password": "password123"
      },
      "newPassword": "pizza123"
    }
    ```

#### Delete User

- **Method**: DELETE
- **Endpoint**: `/user/delete/{id}`
- **Description**: Deletes a user by their ID.
- **Example**: DELETE `/user/delete/456`

## Security

UserService uses HashiCorp Vault to manage secrets securely. JWT secrets are stored in Vault and fetched dynamically, ensuring that sensitive information is handled with care.

## Monitoring & Logging

UserService is integrated with Prometheus, Loki, and Grafana for monitoring and logging. 

### Logging

Logs are sent to Loki via the `LokiEndpoint` for centralized logging.

### Metrics

Application metrics are exposed and can be collected by Prometheus. Grafana is used for visualizing these metrics.

## Continuous Deployment

The CI/CD pipeline configuration will handle automatic deployments via platforms such as Jenkins, GitHub Actions, or GitLab CI. Be sure to set your secrets and configurations in your respective CI/CD environment.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
