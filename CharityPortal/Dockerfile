# Use the official Golang image as the build stage
FROM golang:1.24 AS build

# Set the working directory in the container
WORKDIR /app

# Copy go mod and sum files
COPY go.mod ./
COPY go.sum ./

# Download dependencies
RUN go mod download

# Copy the source code
COPY . .

RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -o main cmd/main.go

# Use a minimal base image for production
FROM alpine:latest

# Set the working directory
WORKDIR /root/

# Copy the compiled binary from the build stage
COPY --from=build /app/main ./
COPY --from=build /app/web/static ./web/static
COPY --from=build /app/config.toml ./config.toml

# Expose application port
EXPOSE 8080

# Command to run the application
CMD ["./main"]

