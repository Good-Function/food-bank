package steps

import "charity_portal/internal/data_confirmation/model"

type AddressStep struct{}

func (a AddressStep) GetStepData() *model.OrgStepData {
	return getAddressDataStep()
}

func NewAddressStep() *AddressStep {
	return &AddressStep{}
}

func (a AddressStep) ValidateStepData() error {
	return nil
}

var AddressStepFieldsMap = map[string]*model.FieldInput{
	"organization": {
		FieldLabel: "Organizacja, która podpisała umowę",
		FieldName:  "organization",
		FiledType:  "text",
		FieldValue: "Odział rejonowy ",
		FieldError: "",
	},
	"registered_address": {
		FieldLabel: "Adres rejestrowy",
		FieldName:  "registered_address",
		FiledType:  "text",
		FieldValue: "ul. Ulicowa 34,",
		FieldError: "",
	},
	"delivery_point": {
		FieldLabel: "Placówka do której trafia żywność",
		FieldName:  "delivery_point",
		FiledType:  "text",
		FieldValue: "Polski związek",
		FieldError: "",
	},
	"delivery_address": {
		FieldLabel: "Adres dostawy żywności",
		FieldName:  "delivery_address",
		FiledType:  "text",
		FieldValue: "ul. Brsadfacka 3 lok. 44, 88-888 Miasto",
		FieldError: "",
	},
	"district": {
		FieldLabel: "Gmina / Dzielnica",
		FieldName:  "district",
		FiledType:  "text",
		FieldValue: "Warszawa",
		FieldError: "",
	},
	"county": {
		FieldLabel: "Powiat",
		FieldName:  "county",
		FiledType:  "text",
		FieldValue: "warszawa",
		FieldError: "",
	},
}

var AddressStepFieldsOrder = []string{
	"organization",
	"registered_address",
	"delivery_point",
	"delivery_address",
	"district",
	"county",
}

func getAddressDataStep() *model.OrgStepData {
	return &model.OrgStepData{
		Title:       "Dane adresowe",
		Fields:      AddressStepFieldsMap,
		FieldsOrder: AddressStepFieldsOrder,
	}
}
