package dataconfirmation

func GetAddressDataStep() *OrgStepData {
	return &OrgStepData{
		Title: "Dane adresowe",
		Labels: []string{
			"Organizacja, która podpisała umowę",
			"Adres rejestrowy",
			"Placówka do której trafia żywność",
			"Adres dostawy żywności",
			"Gmina / Dzielnica",
			"Powiat",
		},
		Types: []string{
			"text",
			"text",
			"text",
			"text",
			"text",
			"text",
		},
		Names: []string{
			"organization",
			"registered_address",
			"delivery_point",
			"delivery_address",
			"district",
			"county",
		},
		Values: []string{
			"Oddział Rejonowy Polskiego Związku Emerytów Rencistów i Inwalidów w Michałowicach",
			"ul. Raszyńska 34,",
			"Polski Związek Emerytów, Rencistów i Inwalidów Koło Terenowe Nr1 w Komorowie",
			"ul. Mari Dąbrowskiej 12/20, 05-806 Komorów",
			"Michałowice",
			"pruszkowski",
		},
	}
}
