package adapters

import (
	"charity_portal/charity_update/queries"
	"context"
	"fmt"
)

type orgReqBody struct {
	Email string `json:"email"`
}

func MakeReadOrganizationInfoBeEmail(fetcher CallOperator) queries.ReadOrganizationIdByEmail {
	return func(ctx context.Context, email string) (queries.OrgInfo, error) {
		var orgInfo queries.OrgInfo
		reqBody := orgReqBody{Email: email}
		if err := fetcher("POST", "/api/organizations/lookup-by-email", reqBody, &orgInfo); err != nil {
			panic(err)
		}
		return orgInfo, nil
	}
}

func MakeFetchKontakty(fetcher CallOperator) queries.ReadKontaktyBy {
	return func(ctx context.Context, id int64) (queries.Kontakty, error) {
		var daneKontakowe queries.Kontakty
		url := fmt.Sprintf("/api/%d/organizations/kontakty", id)
		if err := fetcher("GET", url, nil, &daneKontakowe); err != nil {
			panic(err)
		}
		return daneKontakowe, nil
	}
}