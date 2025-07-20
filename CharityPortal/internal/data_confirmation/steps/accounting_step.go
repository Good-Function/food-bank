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

var AccountingStepFieldsMap = map[string]*model.FieldInput{
	"accounting_organization": {
		FieldLabel: "Organizacja na którą wystawiamy WZ",
		FieldName:  "accounting_organization",
		FiledType:  "text",
		FieldValue: "Polski Związek Emerytów Rencistów i Inwalidów Zarząd Główny",
		FieldError: "",
	},
	"accounting_address": {
		FieldLabel: "Adres",
		FieldName:  "accounting_address",
		FiledType:  "text",
		FieldValue: "ul. Bracka 5 lok. 10, 00-501 Warszawa",
		FieldError: "",
	},
	"accounting_phone": {
		FieldLabel: "Telefon",
		FieldName:  "accounting_phone",
		FiledType:  "text",
		FieldValue: "22 827 09 15\n22 827 28 19",
		FieldError: "",
	},
}

var AccountingStepFieldsOrder = []string{
	"accounting_organization",
	"accounting_address",
	"accounting_phone",
}

func GetAccountingDataStep() *model.OrgStepData {
	return &model.OrgStepData{
		Title:   "Księgowość",
		Fields:      AccountingStepFieldsMap,
		FieldsOrder: AccountingStepFieldsOrder,
	}
}
