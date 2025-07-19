package dataconfirmation

func GetAccountingDataStep() *OrgStepData {
	return &OrgStepData{
		Title: "Dane adresowe księgowości",
		Labels: []string{
			"Organizacja na którą wystawiamy WZ",
			"Adres",
			"Telefon",
		},
		Types: []string{
			"text",
			"text",
			"text",
		},
		Names: []string{
			"accounting_organization",
			"accounting_address",
			"accounting_phone",
		},
		Values: []string{
			"Polski Związek Emerytów Rencistów i Inwalidów Zarząd Główny",
			"ul. Bracka 5 lok. 10, 00-501 Warszawa",
			"22 827 09 15\n22 827 28 19",
		},
	}
}
