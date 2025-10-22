package charityupdate

import (
	"charity_portal/charity_update/operator_api"
	"charity_portal/charity_update/views"
	"charity_portal/charity_update/views/steps"
	"charity_portal/web"
	"charity_portal/web/layout"
	"fmt"
	"log/slog"
	"net/http"
	"strings"
)

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

func sessionOrError(r *http.Request, w http.ResponseWriter) *web.SessionData {
	session := r.Context().Value(web.UserContextKey{}).(*web.SessionData)
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

func daneAdresoweHandler(readDaneAdresoweBy operator_api.ReadDaneAdresoweBy) http.HandlerFunc {
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

func updateDaneAdresoweHandler(updateDaneAdresoweBy operator_api.UpdateDaneAdresoweBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if err := r.ParseForm(); err != nil {
			http.Error(w, "Invalid form data", http.StatusBadRequest)
			return
		}
		session := sessionOrError(r,w)
		daneAdresowe := operator_api.DaneAdresowe{
			NazwaOrganizacjiPodpisujacejUmowe: r.FormValue("NazwaOrganizacjiPodpisujacejUmowe"),
			AdresRejestrowy:                   r.FormValue("AdresRejestrowy"),
			NazwaPlacowkiTrafiaZywnosc:        r.FormValue("NazwaPlacowkiTrafiaZywnosc"),
			AdresPlacowkiTrafiaZywnosc:        r.FormValue("AdresPlacowkiTrafiaZywnosc"),
			GminaDzielnica:                    r.FormValue("GminaDzielnica"),
			Powiat:                            r.FormValue("Powiat"),
		}
		updateDaneAdresoweBy(r.Context(), *session.OrgID, daneAdresowe)
		w.Header().Set("HX-Redirect", "/charity-update/kontakty#kontakty")
		w.WriteHeader(http.StatusOK)
	}
}

func daneKontaktoweHandler(readDaneKontaktoweBy operator_api.ReadKontaktyBy) http.HandlerFunc {
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

func updateKontaktyHandler(updateDaneKontaktoweBy operator_api.UpdateKontaktyBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if err := r.ParseForm(); err != nil {
			http.Error(w, "Invalid form data", http.StatusBadRequest)
			return
		}
		session := sessionOrError(r,w)
		kontakty := operator_api.Kontakty{
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
		w.Header().Set("HX-Redirect", "/charity-update/beneficjenci#beneficjenci")
		w.WriteHeader(http.StatusOK)
	}
}

func beneficjenciHandler(readDaneKontaktoweBy operator_api.ReadBeneficjenciBy) http.HandlerFunc {
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

func updateBeneficjenciHandler(updateBeneficjenciBy operator_api.UpdateBeneficjenciBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if err := r.ParseForm(); err != nil {
			http.Error(w, "Invalid form data", http.StatusBadRequest)
			return
		}
		session := sessionOrError(r,w)
		beneficjenci := operator_api.Beneficjenci{
			LiczbaBeneficjentow: func() int {
				var val int
				fmt.Sscanf(r.FormValue("LiczbaBeneficjentow"), "%d", &val)
				return val
			}(),
			Beneficjenci: r.FormValue("Beneficjenci"),
		}
		updateBeneficjenciBy(r.Context(), *session.OrgID, beneficjenci)
		w.Header().Set("HX-Redirect", "/charity-update/zrodla-zywnosci#zrodla-zywnosci")
		w.WriteHeader(http.StatusOK)
	}
}

func zrodlaZywnosciHandler(readZrodlaZywnosciBy operator_api.ReadZrodlaZywnosciBy) http.HandlerFunc {
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

func updateZrodlaZywnosciHandler(updateZrodlaZywnosciBy operator_api.UpdateZrodlaZywnosciBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if err := r.ParseForm(); err != nil {
			http.Error(w, "Invalid form data", http.StatusBadRequest)
			return
		}
		session := sessionOrError(r,w)
		zrodla := operator_api.ZrodlaZywnosci{
			Sieci:              r.FormValue("Sieci") == "on",
			Bazarki:            r.FormValue("Bazarki") == "on",
			Machfit:            r.FormValue("Machfit") == "on",
			FEPZ2024:           r.FormValue("FEPZ2024") == "on",
			OdbiorKrotkiTermin: r.FormValue("OdbiorKrotkiTermin") == "on",
			TylkoNaszMagazyn:   r.FormValue("TylkoNaszMagazyn") == "on",
		}
		updateZrodlaZywnosciBy(r.Context(), *session.OrgID, zrodla)
		w.Header().Set("HX-Redirect", "/charity-update/warunki-udzielania-pomocy#warunki-udzielania-pomocy")
		w.WriteHeader(http.StatusOK)
	}
}

func warunkiUdzielaniaPomocyHandler(readWarunkiPomocy operator_api.ReadWarunkiPomocyBy) http.HandlerFunc {
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

func updateWarunkiUdzielaniaPomocyHandler(updateWarunkiPomocyBy operator_api.UpdateWarunkiPomocyBy) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if err := r.ParseForm(); err != nil {
			http.Error(w, "Invalid form data", http.StatusBadRequest)
			return
		}
		session := sessionOrError(r, w)
		warunki := operator_api.WarunkiPomocy{
			Kategoria: 			r.FormValue("Kategoria"),
			RodzajPomocy: 		r.FormValue("RodzajPomocy"),
			SposobUdzielaniaPomocy: r.FormValue("SposobUdzielaniaPomocy"),
			WarunkiMagazynowe: 	r.FormValue("WarunkiMagazynowe"),
			HACCP:              r.FormValue("HACCP") == "on",
			Sanepid:            r.FormValue("Sanepid") == "on",
			TransportOpis:      r.FormValue("TransportOpis"),
			TransportKategoria: r.FormValue("TransportKategoria"),
		}
		updateWarunkiPomocyBy(r.Context(), *session.OrgID, warunki)
		w.Header().Set("HX-Redirect", "/charity-update")
		w.WriteHeader(http.StatusOK)
	}
}

func CreateRouter(dependencies *dependencies) *http.ServeMux {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /", wizardHandler)
	mux.HandleFunc("GET /dane-adresowe-form", daneAdresoweHandler(dependencies.readDaneAdresoweBy))
	mux.HandleFunc("PUT /dane-adresowe-form", updateDaneAdresoweHandler(dependencies.updateDaneAdresoweBy))
	mux.HandleFunc("GET /dane-adresowe", wizardHandler)
	mux.HandleFunc("GET /kontakty-form", daneKontaktoweHandler(dependencies.readDaneKontaktoweBy))
	mux.HandleFunc("PUT /kontakty-form", updateKontaktyHandler(dependencies.updateDaneKontaktoweBy))
	mux.HandleFunc("GET /kontakty", wizardHandler)
	mux.HandleFunc("GET /beneficjenci-form", beneficjenciHandler(dependencies.readBeneficjenciBy))
	mux.HandleFunc("PUT /beneficjenci-form", updateBeneficjenciHandler(dependencies.updateBeneficjenciBy))
	mux.HandleFunc("GET /beneficjenci", wizardHandler)
	mux.HandleFunc("GET /zrodla-zywnosci-form", zrodlaZywnosciHandler(dependencies.readZrodlaZywnosciBy))
	mux.HandleFunc("PUT /zrodla-zywnosci-form", updateZrodlaZywnosciHandler(dependencies.updateZrodlaZywnosciBy))
	mux.HandleFunc("GET /zrodla-zywnosci", wizardHandler)
	mux.HandleFunc("GET /warunki-udzielania-pomocy-form", warunkiUdzielaniaPomocyHandler(dependencies.readWarunkiUdzielaniaPomocyBy))
	mux.HandleFunc("PUT /warunki-udzielania-pomocy-form", updateWarunkiUdzielaniaPomocyHandler(dependencies.updateWarunkiUdzielaniaPomocyBy))
	mux.HandleFunc("GET /warunki-udzielania-pomocy", wizardHandler)
	mux.Handle("GET /finito", finitoHandler())
	return mux
}