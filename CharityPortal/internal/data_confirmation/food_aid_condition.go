package dataconfirmation

func GetFoodAidConditionsStep() *OrgStepData {
	return &OrgStepData{
		Title: "Warunki udzielania pomocy żywnościowej",
		Labels: []string{
			"Kategoria",
			"Rodzaj pomocy",
			"Sposób udzielania pomocy",
			"Warunki magazynowe",
			"HACCP",
			"Sanepid",
			"Transport - opis",
			"Transport - kategoria",
		},
		Types: []string{
			"text",
			"text",
			"text",
			"text",
			"text",
			"text",
			"text",
			"text",
		},
		Names: []string{
			"category",
			"aid_type",
			"aid_method",
			"storage_conditions",
			"haccp",
			"sanepid",
			"transport_description",
			"transport_category",
		},
		Values: []string{
			"paczki",
			"P",
			"",
			"",
			"Nie",
			"Nie",
			"",
			"",
		},
	}

}
