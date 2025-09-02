package charityupdate

import (
	"context"
	"io"
	"net/http"

	"log/slog"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore/policy"
	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
)

func TestFetch() (*string, error) {
    cred, err := azidentity.NewDefaultAzureCredential(nil)
    if err != nil {
        slog.Error("Can't create credentials", "err", err)
		return nil, err
    }

    token, err := cred.GetToken(context.Background(), policy.TokenRequestOptions{
        Scopes: []string{"api://03241880-d8b0-408f-800e-1a0aec3e8746/access_as_app"},
    })
    if err != nil {
        slog.Error("Can't get token", "err", err)
		return nil, err
    }

    req, _ := http.NewRequest("GET", "https://charity-portal.azurecontainerapps.io/api/ping", nil)
    req.Header.Set("Authorization", "Bearer "+token.Token)

    client := &http.Client{}
    resp, err := client.Do(req)
    if err != nil {
        slog.Error("Can't get response", "err", err)
		return nil, err
    }
    defer resp.Body.Close()

	bodyBytes, err := io.ReadAll(resp.Body)
	if err != nil {
		slog.Error("Can't read body", "err", err)
		return nil, err
	}
	responseString := (string(bodyBytes))
    return &responseString, nil
}
