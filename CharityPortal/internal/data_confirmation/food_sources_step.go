package dataconfirmation

func GetFoodSourcesStep() *OrgStepData {
	return &OrgStepData{
		Title: "Źródła żywności",
		Labels: []string{
			"Sieci",
			"Bazarki",
			"Machfit",
			"FEPŻ 2024",
			"Odbiór Krótki Termin",
			"Tylko nasz magazyn",
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
			"source_networks",
			"source_markets",
			"source_machfit",
			"source_fepz",
			"source_short_term",
			"source_internal_only",
		},
		Values: []string{
			"Nie",
			"Nie",
			"Nie",
			"Nie",
			"Nie",
			"Nie",
		},
	}
}
