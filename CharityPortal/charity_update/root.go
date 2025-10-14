package charityupdate

import (
	"charity_portal/charity_update/adapters"
	"charity_portal/charity_update/queries"
)

type dependencies = struct {
	readDaneKontaktoweBy queries.ReadKontaktyBy
	updateDaneKontaktoweBy queries.UpdateKontaktyBy
	readDaneAdresoweBy queries.ReadDaneAdresoweBy
	updateDaneAdresoweBy queries.UpdateDaneAdresoweBy
	readBeneficjenciBy   queries.ReadBeneficjenciBy
	updateBeneficjenciBy queries.UpdateBeneficjenciBy
	readAdresyKsiegowosciBy   queries.ReadAdresyKsiegowosciBy
	updateAdresyKsiegowosciBy queries.UpdateAdresyKsiegowosciBy
	readZrodlaZywnosciBy   queries.ReadZrodlaZywnosciBy
	updateZrodlaZywnosciBy queries.UpdateZrodlaZywnosciBy
	readWarunkiUdzielaniaPomocyBy   queries.ReadWarunkiPomocyBy
	updateWarunkiUdzielaniaPomocyBy queries.UpdateWarunkiPomocyBy
}

func Compose(config Config) *dependencies {
	var callOperator adapters.CallOperator
	if config.MockOperatorApi {
		callOperator = adapters.CallOperatorMock
	} else {
		callOperator = adapters.MakeCallOperator(config.OperatorApiClientId, config.OperatorApiBaseUrl)
	}

	return &dependencies{
		readDaneKontaktoweBy: adapters.MakeFetcher[queries.Kontakty](callOperator, "/api/organizations/%d/kontakty"),
		updateDaneKontaktoweBy: adapters.MakeUpdater[queries.Kontakty](callOperator, "/api/organizations/%d/kontakty"),
		readDaneAdresoweBy: adapters.MakeFetcher[queries.DaneAdresowe](callOperator, "/api/organizations/%d/dane-adresowe"),
		updateDaneAdresoweBy: adapters.MakeUpdater[queries.DaneAdresowe](callOperator, "/api/organizations/%d/dane-adresowe"),
		readBeneficjenciBy: adapters.MakeFetcher[queries.Beneficjenci](callOperator, "/api/organizations/%d/beneficjenci"),
		updateBeneficjenciBy: adapters.MakeUpdater[queries.Beneficjenci](callOperator, "/api/organizations/%d/beneficjenci"),
		readAdresyKsiegowosciBy: adapters.MakeFetcher[queries.AdresyKsiegowosci](callOperator, "/api/organizations/%d/adresy-ksiegowosci"),
		updateAdresyKsiegowosciBy: adapters.MakeUpdater[queries.AdresyKsiegowosci](callOperator, "/api/organizations/%d/adresy-ksiegowosci"),
		readZrodlaZywnosciBy: adapters.MakeFetcher[queries.ZrodlaZywnosci](callOperator, "/api/organizations/%d/zrodla-zywnosci"),
		updateZrodlaZywnosciBy: adapters.MakeUpdater[queries.ZrodlaZywnosci](callOperator, "/api/organizations/%d/zrodla-zywnosci"),
		readWarunkiUdzielaniaPomocyBy: adapters.MakeFetcher[queries.WarunkiPomocy](callOperator, "/api/organizations/%d/warunki-pomocy"),
		updateWarunkiUdzielaniaPomocyBy: adapters.MakeUpdater[queries.WarunkiPomocy](callOperator, "/api/organizations/%d/warunki-pomocy"),
	}
}