package steps

import "charity_portal/internal/data_confirmation/model"

type AccountingStep struct{}

func (s *AccountingStep) GetStepData() *model.OrgStepData {
	return GetAccountingDataStep()
}

func NewAccountingStep() *AccountingStep {
	return &AccountingStep{}
}

func (s *AccountingStep) ValidateStepData() error {
	// Implement validation logic if needed
	return nil
}

func GetAccountingDataStep() *model.OrgStepData {
	return &model.OrgStepData{
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
