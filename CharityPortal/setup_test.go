package charityportal_test

import (
	"context"
	"fmt"
	"log"
	"os"
	"os/exec"
	"strings"
	"testing"

	tc "github.com/testcontainers/testcontainers-go"
	"github.com/testcontainers/testcontainers-go/wait"
)

var container tc.Container

func init() {
    os.Setenv("TESTCONTAINERS_RYUK_DISABLED", "true")
}

func TestMain(m *testing.M) {
	ctx := context.Background()
	containerName := "foodbank-db"

	if !isContainerRunning(containerName) {
		fmt.Println("Starting PostgreSQL container...")
		req := tc.ContainerRequest{
			Name:         containerName,
			Image:        "postgres:16-alpine",
			Env: map[string]string{
				"POSTGRES_USER":     "charity_user",
				"POSTGRES_PASSWORD": "charity_pass",
				"POSTGRES_DB":       "testdb",
			},
			ExposedPorts: []string{"5432/tcp"},
			WaitingFor:   wait.ForListeningPort("5432/tcp"),
		}

		var err error
		container, err = tc.GenericContainer(ctx, tc.GenericContainerRequest{
			ContainerRequest: req,
			Started:          true,
		})
		if err != nil {
			log.Fatalf("failed to start container: %v", err)
		}

		host, _ := container.Host(ctx)
		port, _ := container.MappedPort(ctx, "5432")
		fmt.Printf("Postgres started at %s:%s\n", host, port.Port())
	} else {
		fmt.Println("PostgreSQL container already running, skipping creation.")
	}

	code := m.Run()
	os.Exit(code)
}

func isContainerRunning(name string) bool {
	cmd := exec.Command("docker", "ps", "--filter", "name="+name, "--filter", "status=running", "--format", "{{.Names}}")
	out, err := cmd.Output()
	if err != nil {
		return false
	}
	return strings.TrimSpace(string(out)) == name
}
