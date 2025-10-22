package charityupdate

import (
	"charity_portal/charity_update/adapters"
	"charity_portal/charity_update/operator_api"
)

type dependencies = struct {
	readDaneKontaktoweBy operator_api.ReadKontaktyBy
	updateDaneKontaktoweBy operator_api.UpdateKontaktyBy
	readDaneAdresoweBy operator_api.ReadDaneAdresoweBy
	updateDaneAdresoweBy operator_api.UpdateDaneAdresoweBy
	readBeneficjenciBy   operator_api.ReadBeneficjenciBy
	updateBeneficjenciBy operator_api.UpdateBeneficjenciBy
	readAdresyKsiegowosciBy   operator_api.ReadAdresyKsiegowosciBy
	updateAdresyKsiegowosciBy operator_api.UpdateAdresyKsiegowosciBy
	readZrodlaZywnosciBy   operator_api.ReadZrodlaZywnosciBy
	updateZrodlaZywnosciBy operator_api.UpdateZrodlaZywnosciBy
	readWarunkiUdzielaniaPomocyBy   operator_api.ReadWarunkiPomocyBy
	updateWarunkiUdzielaniaPomocyBy operator_api.UpdateWarunkiPomocyBy
}

func Compose(config Config) *dependencies {
	var callOperator adapters.CallOperator
	if config.MockOperatorApi {
		callOperator = adapters.CallOperatorMock
	} else {
		callOperator = adapters.MakeCallOperator(config.OperatorApiClientId, config.OperatorApiBaseUrl)
	}

	return &dependencies{
		readDaneKontaktoweBy: adapters.MakeFetcher[operator_api.Kontakty](callOperator, "/api/organizations/%d/kontakty"),
		updateDaneKontaktoweBy: adapters.MakeUpdater[operator_api.Kontakty](callOperator, "/api/organizations/%d/kontakty"),
		readDaneAdresoweBy: adapters.MakeFetcher[operator_api.DaneAdresowe](callOperator, "/api/organizations/%d/dane-adresowe"),
		updateDaneAdresoweBy: adapters.MakeUpdater[operator_api.DaneAdresowe](callOperator, "/api/organizations/%d/dane-adresowe"),
		readBeneficjenciBy: adapters.MakeFetcher[operator_api.Beneficjenci](callOperator, "/api/organizations/%d/beneficjenci"),
		updateBeneficjenciBy: adapters.MakeUpdater[operator_api.Beneficjenci](callOperator, "/api/organizations/%d/beneficjenci"),
		readAdresyKsiegowosciBy: adapters.MakeFetcher[operator_api.AdresyKsiegowosci](callOperator, "/api/organizations/%d/adresy-ksiegowosci"),
		updateAdresyKsiegowosciBy: adapters.MakeUpdater[operator_api.AdresyKsiegowosci](callOperator, "/api/organizations/%d/adresy-ksiegowosci"),
		readZrodlaZywnosciBy: adapters.MakeFetcher[operator_api.ZrodlaZywnosci](callOperator, "/api/organizations/%d/zrodla-zywnosci"),
		updateZrodlaZywnosciBy: adapters.MakeUpdater[operator_api.ZrodlaZywnosci](callOperator, "/api/organizations/%d/zrodla-zywnosci"),
		readWarunkiUdzielaniaPomocyBy: adapters.MakeFetcher[operator_api.WarunkiPomocy](callOperator, "/api/organizations/%d/warunki-pomocy"),
		updateWarunkiUdzielaniaPomocyBy: adapters.MakeUpdater[operator_api.WarunkiPomocy](callOperator, "/api/organizations/%d/warunki-pomocy"),
	}
}