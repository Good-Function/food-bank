package dataconfirmation

func GetBeneficiariesDataStep() *OrgStepData{
	return &OrgStepData{
		Title: "Beneficjenci",
		Labels: []string{
			"Liczba Beneficjentów",
			"Beneficjenci",
		},
		Types: []string{
			"number",
			"text",
		},
		Names: []string{
			"beneficiary_count",
			"beneficiary_group",
		},
		Values: []string{
			"100",
			"seniorzy",
		},
	}
}
