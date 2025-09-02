package database

import "context"

type ReadCharityByEmail func(ctx context.Context, email string) (Organizacje, error)
