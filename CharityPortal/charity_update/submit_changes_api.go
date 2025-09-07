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
	}
    token, err := cred.GetToken(context.Background(), policy.TokenRequestOptions{
        Scopes: []string{"api://03241880-d8b0-408f-800e-1a0aec3e8746/.default"},
    })
	if err != nil {
        slog.Error("Failed to get token", "err", err)
	}
    client := &http.Client{}
	req, err := http.NewRequest("GET", "https://operator-portal.bluemeadow-0985b16b.polandcentral.azurecontainerapps.io/api/pingp", nil)
    // internal?
	if err != nil {
        slog.Error("Failed to create request", "err", err)
	}
    req.Header.Add("Authorization", "Bearer "+token.Token)
    resp, err := client.Do(req)
	if err != nil {
        slog.Error("failed to send request", "err", err)
	}
	defer resp.Body.Close()


	body, err := io.ReadAll(resp.Body)
    if err != nil {
        slog.Error("Failed read body", "err", err)
    }
	responseString := (string(body))
    return &responseString, nil
}