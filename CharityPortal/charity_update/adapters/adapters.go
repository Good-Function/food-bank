package adapters

import (
	"charity_portal/charity_update/queries"
	"context"
	"fmt"
)

type orgReqBody struct {
	Email string `json:"email"`
}

func MakeReadOrganizationInfoByEmail(fetcher CallOperator) queries.ReadOrganizationIdByEmail {
	return func(ctx context.Context, email string) (queries.OrgInfo, error) {
		var orgInfo queries.OrgInfo
		reqBody := orgReqBody{Email: email}
		if err := fetcher("POST", "/api/organizations/lookup-by-email", reqBody, &orgInfo); err != nil {
			panic(err)
		}
		return orgInfo, nil
	}
}

func MakeUpdater[T any](fetcher CallOperator, pathTemplate string) func(ctx context.Context, id int64, payload T) error {
	return func(ctx context.Context, id int64, payload T) error {
		url := fmt.Sprintf(pathTemplate, id)
		if err := fetcher("PUT", url, payload, nil); err != nil {
			panic(err)
		}
		return nil
	}
}

func MakeFetcher[T any](fetcher CallOperator, pathTemplate string) func(ctx context.Context, id int64) (T, error) {
	return func(ctx context.Context, id int64) (T, error) {
		var data T
		url := fmt.Sprintf(pathTemplate, id)
		if err := fetcher("GET", url, nil, &data); err != nil {
			panic(err)
		}
		return data, nil
	}
}
