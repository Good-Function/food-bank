package adapters

import (
	"context"
	"fmt"
	"io"
	"net/http"

	"encoding/json"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore/policy"
	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
)


type CallOperator = func(method, url string, out any) error

func AuthenticateForJwt(operatorApiClientId string) (*string, error) {
	cred, err := azidentity.NewDefaultAzureCredential(nil)
	if err != nil {
		return nil, fmt.Errorf("Failed to create credentials", err)
	}
    token, err := cred.GetToken(context.Background(), policy.TokenRequestOptions{
        Scopes: []string{ fmt.Sprintf("api://%s/.default", operatorApiClientId)},
    })
	if err != nil {
		return nil, fmt.Errorf("Failed to obtain token", err)
	}
	return &token.Token, nil
}

func MakeCallOperator(operatorApiClientId, baseUrl string) CallOperator {
	return func(method, url string, out any) error {
		token, err := AuthenticateForJwt(operatorApiClientId)
		if err != nil {
			return err
		}

		client := &http.Client{}
		req, err := http.NewRequest(method, baseUrl + url, nil)
		if err != nil {
			return fmt.Errorf("failed to create request: %w", err)
		}
		req.Header.Add("Authorization", "Bearer "+*token)

		resp, err := client.Do(req)
		if err != nil {
			return fmt.Errorf("failed to send request: %w", err)
		}
		defer resp.Body.Close()

		if resp.StatusCode != http.StatusOK {
			body, _ := io.ReadAll(resp.Body)
			return fmt.Errorf("non-200 status %d: %s", resp.StatusCode, string(body))
		}

		body, err := io.ReadAll(resp.Body)
		if err != nil {
			return fmt.Errorf("failed to read body: %w", err)
		}

		if err := json.Unmarshal(body, out); err != nil {
			return fmt.Errorf("failed to unmarshal response: %w", err)
		}

		return nil
	}
}
