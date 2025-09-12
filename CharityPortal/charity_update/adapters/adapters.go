package adapters

import (
	"charity_portal/charity_update/queries"
	"context"
	"fmt"
)

func MakeFetchKontakty(fetcher CallOperator) queries.ReadKontaktyBy {
	return func(ctx context.Context, email string) (queries.Kontakty, error) {
		var daneKontakowe queries.Kontakty
		// todo HMAC with secret
		url := fmt.Sprintf("/api/organizations/kontakty?email=%s", email)
		if err := fetcher("GET", url, &daneKontakowe); err != nil {
			panic(err)
		}
		return daneKontakowe, nil
	}
}