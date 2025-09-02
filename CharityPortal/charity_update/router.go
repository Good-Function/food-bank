package charityupdate

import (
	"charity_portal/charity_update/database"
	"charity_portal/web/views"
	"context"
	"fmt"
	"net/http"

	"log/slog"

	"github.com/jackc/pgx/v5/pgxpool"
)

type dependencies = struct {
	readCharityByEmail database.ReadCharityByEmail
}

func Compose(operatorDbConnectionPool *pgxpool.Pool, environment string) *dependencies {
	var readCharityByEmail database.ReadCharityByEmail
	if environment == "development" {
		readCharityByEmail = database.ReadCharityByEmailMock
	} else {
		readCharityByEmail = database.New(operatorDbConnectionPool).ReadCharity
	}
	return &dependencies{
		readCharityByEmail:readCharityByEmail,
	}
}

func welcomeHandler(readCharity func (ctx context.Context, email string) (database.Organizacje, error)) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		charity, err := readCharity(r.Context(), "domopieki2@poczta.onet.pl")
		if err != nil {
			slog.Error("Can't read organization", "err", err)
		}
		orgStr := fmt.Sprintf("%+v\n", charity)
		views.Base(Welcome(orgStr), "").Render(r.Context(), w)
	}
}

func operatorHandler() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		operatorResponse, err := TestFetch()
		if err != nil {
			slog.Error("Can't read fetch", "err", err)
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
		views.Base(Welcome(*operatorResponse), "").Render(r.Context(), w)
	}
}

func CreateRouter(dependencies *dependencies) *http.ServeMux {
	mux := http.NewServeMux()
	mux.Handle("GET /operator", operatorHandler())
	mux.Handle("GET /", welcomeHandler(dependencies.readCharityByEmail))
	return mux
}