package charityupdate

import (
	"charity_portal/charity_update/queries"
	"charity_portal/charity_update/views"
	"charity_portal/charity_update/views/steps"
	"charity_portal/internal/auth"
	"charity_portal/web/layout"
	"log/slog"
	"net/http"
	"strings"
)

// Todo:
// 1. Renderować dobre następny stepy po zapisaniu
// 3. Rozszezyć update
// 4. Rozszerzyć formularze

func ParseStep(s string) views.WizardStep {
	switch s {
	case "dane-adresowe":
		return views.DaneAdresowe
	case "kontakty":
		return views.Kontakty
	case "beneficjenci":
		return views.Beneficjenci
	case "zrodla-zywnosci":
		return views.ZrodlaZywnosci
	case "warunki-udzielania-pomocy":
		return views.WarunkiUdzielaniaPomocy
	default:
		return views.Start
	}
}

func sessionOrError(r *http.Request, w http.ResponseWriter) *auth.SessionData {
	session := r.Context().Value(auth.UserContextKey{}).(*auth.SessionData)
	if session == nil || session.OrgID == nil {
		http.Error(w, "Unauthorized", http.StatusUnauthorized)
		return nil
	}
	return session
}

func finitoHandler() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Header.Get("HX-Request") != "" {
			views.Finito().Render(r.Context(), w)
		} else {
			layout.Base(views.Finito(), "").Render(r.Context(), w)
		}
		
	}
}

func wizardHandler(w http.ResponseWriter, r *http.Request) {
		parts := strings.Split(strings.Trim(r.URL.Path, "/"), "/")
		var step views.WizardStep
		if len(parts) > 0 {
			step = ParseStep(parts[0])
		} else {
			step = views.Start
		}
		if r.Header.Get("HX-Request") != "" {
			views.Wizard(step).Render(r.Context(), w)
		} else {
			layout.Base(views.Wizard(step), "").Render(r.Context(), w)
		}	
	}

func daneAdresoweHandler(readDaneAdresoweBy queries.ReadDaneAdresoweBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		session := sessionOrError(r,w)
		daneAdresowe, err := readDaneAdresoweBy(r.Context(), *session.OrgID)
		if err != nil {
			slog.Error("Can't read organization", "err", err)
		}
		if r.Header.Get("HX-Request") != "" {
			steps.DaneAdresoweForm(daneAdresowe, *session.OrgID).Render(r.Context(), w)
		} else {
			layout.Base(steps.DaneAdresoweForm(daneAdresowe, *session.OrgID), "").Render(r.Context(), w)
		}
	}
}

func daneKontaktoweHandler(readDaneKontaktoweBy queries.ReadKontaktyBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		session := sessionOrError(r,w)
		kontakty, err := readDaneKontaktoweBy(r.Context(), *session.OrgID)
		if err != nil {
			slog.Error("Can't read organization", "err", err)
		}
		if r.Header.Get("HX-Request") != "" {
			steps.KontaktyForm(kontakty, *session.OrgID).Render(r.Context(), w)
		} else {
			layout.Base(steps.KontaktyForm(kontakty, *session.OrgID), "").Render(r.Context(), w)
		}
	}
}

func beneficjenciHandler(readDaneKontaktoweBy queries.ReadBeneficjenciBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		session := sessionOrError(r,w)
		beneficjenci, err := readDaneKontaktoweBy(r.Context(), *session.OrgID)
		if err != nil {
			slog.Error("Can't read organization", "err", err)
		}
		if r.Header.Get("HX-Request") != "" {
			steps.BeneficenciForm(beneficjenci, *session.OrgID).Render(r.Context(), w)
		} else {
			layout.Base(steps.BeneficenciForm(beneficjenci, *session.OrgID), "").Render(r.Context(), w)
		}
	}
}

func zrodlaZywnosciHandler(readZrodlaZywnosciBy queries.ReadZrodlaZywnosciBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		session := sessionOrError(r,w)
		zrodla, err := readZrodlaZywnosciBy(r.Context(), *session.OrgID)
		if err != nil {
			slog.Error("Can't read organization", "err", err)
		}
		if r.Header.Get("HX-Request") != "" {
			steps.ZrodlaZywnosciForm(zrodla, *session.OrgID).Render(r.Context(), w)
		} else {
			layout.Base(steps.ZrodlaZywnosciForm(zrodla, *session.OrgID), "").Render(r.Context(), w)
		}
	}
}

func warunkiUdzielaniaPomocyHandler(readWarunkiPomocy queries.ReadWarunkiPomocyBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		session := sessionOrError(r, w)
		warunki, err := readWarunkiPomocy(r.Context(), *session.OrgID)
		if err != nil {
			slog.Error("Can't read organization", "err", err)
		}
		if r.Header.Get("HX-Request") != "" {
			steps.WarunkiUdzielaniaPomocy(warunki, *session.OrgID).Render(r.Context(), w)
		} else {
			layout.Base(steps.WarunkiUdzielaniaPomocy(warunki, *session.OrgID), "").Render(r.Context(), w)
		}
	}
}

func updateKontaktyHandler(updateDaneKontaktoweBy queries.UpdateKontaktyBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if err := r.ParseForm(); err != nil {
			http.Error(w, "Invalid form data", http.StatusBadRequest)
			return
		}
		session := sessionOrError(r,w)
		kontakty := queries.Kontakty{
			WwwFacebook:              r.FormValue("WwwFacebook"),
			Telefon:                  r.FormValue("Telefon"),
			Przedstawiciel:           r.FormValue("Przedstawiciel"),
			Kontakt:                  r.FormValue("Kontakt"),
			Email:                    r.FormValue("Email"),
			Dostepnosc:               r.FormValue("Dostepnosc"),
			OsobaDoKontaktu:          r.FormValue("OsobaDoKontaktu"),
			TelefonOsobyKontaktowej:  r.FormValue("TelefonOsobyKontaktowej"),
			MailOsobyKontaktowej:     r.FormValue("MailOsobyKontaktowej"),
			OsobaOdbierajacaZywnosc:  r.FormValue("OsobaOdbierajacaZywnosc"),
			TelefonOsobyOdbierajacej: r.FormValue("TelefonOsobyOdbierajacej"),
		}
		updateDaneKontaktoweBy(r.Context(), *session.OrgID, kontakty)
		views.Wizard(views.Beneficjenci).Render(r.Context(), w)
	}
}

func CreateRouter(dependencies *dependencies) *http.ServeMux {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /", wizardHandler)
	mux.HandleFunc("GET /dane-adresowe-form", daneAdresoweHandler(dependencies.readDaneAdresoweBy))
	mux.HandleFunc("GET /dane-adresowe", wizardHandler)
	mux.HandleFunc("GET /kontakty-form", daneKontaktoweHandler(dependencies.readDaneKontaktoweBy))
	mux.HandleFunc("PUT /kontakty-form", updateKontaktyHandler(dependencies.updateDaneKontaktoweBy))
	mux.HandleFunc("GET /kontakty", wizardHandler)
	mux.HandleFunc("GET /beneficjenci-form", beneficjenciHandler(dependencies.readBeneficjenciBy))
	mux.HandleFunc("GET /beneficjenci", wizardHandler)
	mux.HandleFunc("GET /zrodla-zywnosci-form", zrodlaZywnosciHandler(dependencies.readZrodlaZywnosciBy))
	mux.HandleFunc("GET /zrodla-zywnosci", wizardHandler)
	mux.HandleFunc("GET /warunki-udzielania-pomocy-form", warunkiUdzielaniaPomocyHandler(dependencies.readWarunkiUdzielaniaPomocyBy))
	mux.HandleFunc("GET /warunki-udzielania-pomocy", wizardHandler)
	mux.Handle("GET /finito", finitoHandler())
	return mux
}