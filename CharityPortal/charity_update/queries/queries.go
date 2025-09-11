package queries

import "context"

type DaneKontakowe struct {
	Adres string
}

type ReadDaneKontaktoweBy func(context.Context, string) (DaneKontakowe, error)