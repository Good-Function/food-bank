package adapters

import (
	"charity_portal/charity_update/queries"
	"context"
)

func MśakeFetchDaneKontakowe(fetcher CallOperator) queries.ReadDaneKontaktoweBy {
	return func(ctx context.Context, email string) (queries.DaneKontakowe, error) {
		var daneKontakowe queries.DaneKontakowe
		if err := fetcher("GET", "/api/dane-kontakowe&email=email", &daneKontakowe); err != nil {
			panic(err)
		}
		return daneKontakowe, nil
	}
}