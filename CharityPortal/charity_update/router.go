package charityupdate

import (
	"charity_portal/charity_update/adapters"
	"charity_portal/charity_update/queries"
	"charity_portal/internal/auth"
	"charity_portal/web/views"
	"fmt"
	"log/slog"
	"net/http"
)

type dependencies = struct {
	readDaneKontaktoweBy queries.ReadKontaktyBy
}

func Compose(config Config) *dependencies {
	var callOperator adapters.CallOperator
	if config.MockOperatorApi {
		callOperator = adapters.CallOperatorMock
	} else {
		callOperator = adapters.MakeCallOperator(config.OperatorApiClientId, config.OperatorApiBaseUrl)
	}

	return &dependencies{
		readDaneKontaktoweBy: adapters.MakeFetchKontakty(callOperator),
	}
}

func welcomeHandler(readDaneKontaktoweBy queries.ReadKontaktyBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		session := r.Context().Value(auth.UserContextKey{}).(*auth.SessionData)
		if session == nil || session.OrgID == nil {
			http.Error(w, "Unauthorized", http.StatusUnauthorized)
			return
		}
		charity, err := readDaneKontaktoweBy(r.Context(), *session.OrgID)
		if err != nil {
			slog.Error("Can't read organization", "err", err)
		}
		orgStr := fmt.Sprintf("%+v\n", charity)
		views.Base(Welcome(orgStr), "").Render(r.Context(), w)
	}
}

func CreateRouter(dependencies *dependencies) *http.ServeMux {
	mux := http.NewServeMux()
	mux.Handle("GET /", welcomeHandler(dependencies.readDaneKontaktoweBy))
	return mux
}