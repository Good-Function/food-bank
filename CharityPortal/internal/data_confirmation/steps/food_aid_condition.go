package steps

import "charity_portal/internal/data_confirmation/model"

type FoodAidConditionsStep struct{}

func (s *FoodAidConditionsStep) GetStepData() *model.OrgStepData {
	return GetFoodAidConditionsStep()
}

func (s *FoodAidConditionsStep) ValidateStepData() error {
	// Implement validation logic if needed
	return nil
}

func NewFoodAidConditionsStep() *FoodAidConditionsStep {
	return &FoodAidConditionsStep{}
}

var FoodAidConditionsStepFieldsMap = map[string]*model.FieldInput{
	"category": {
		FieldLabel: "Kategoria",
		FieldName:  "category",
		FiledType:  "text",
		FieldValue: "paczki",
		FieldError: "",
	},
	"aid_type": {
		FieldLabel: "Rodzaj pomocy",
		FieldName:  "aid_type",
		FiledType:  "text",
		FieldValue: "P",
		FieldError: "",
	},
	"aid_method": {
		FieldLabel: "Sposób udzielania pomocy",
		FieldName:  "aid_method",
		FiledType:  "text",
		FieldValue: "",
		FieldError: "",
	},
	"storage_conditions": {
		FieldLabel: "Warunki magazynowe",
		FieldName:  "storage_conditions",
		FiledType:  "text",
		FieldValue: "",
		FieldError: "",
	},
	"haccp": {
		FieldLabel: "HACCP",
		FieldName:  "haccp",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
	"sanepid": {
		FieldLabel: "Sanepid",
		FieldName:  "sanepid",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
	"transport_description": {
		FieldLabel: "Transport - opis",
		FieldName:  "transport_description",
		FiledType:  "text",
		FieldValue: "",
		FieldError: "",
	},
	"transport_category": {
		FieldLabel: "Transport - kategoria",
		FieldName:  "transport_category",
		FiledType:  "text",
		FieldValue: "",
		FieldError: "",
	},
}

var FoodAidConditionsStepFieldsOrder = []string{
	"category",
	"aid_type",
	"aid_method",
	"storage_conditions",
	"haccp",
	"sanepid",
	"transport_description",
	"transport_category",
}

func GetFoodAidConditionsStep() *model.OrgStepData {
	return &model.OrgStepData{
		Title:   "Warunki udzielania pomocy żywnościowej",
		Fields:      FoodAidConditionsStepFieldsMap,
		FieldsOrder: FoodAidConditionsStepFieldsOrder,
	}
}
