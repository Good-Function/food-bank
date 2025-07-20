package steps

import "charity_portal/internal/data_confirmation/model"

type FoodSourcesStep struct{}

func (s *FoodSourcesStep) GetStepData() *model.OrgStepData {
	return GetFoodSourcesStep()
}

func (s *FoodSourcesStep) ValidateStepData() error {
	// Implement validation logic if needed
	return nil
}

func NewFoodSourcesStep() *FoodSourcesStep {
	return &FoodSourcesStep{}
}

var FoodSourcesStepFieldsMap = map[string]*model.FieldInput{
	"source_networks": {
		FieldLabel: "Sieci",
		FieldName:  "source_networks",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
	"source_markets": {
		FieldLabel: "Bazarki",
		FieldName:  "source_markets",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
	"source_machfit": {
		FieldLabel: "Machfit",
		FieldName:  "source_machfit",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
	"source_fepz": {
		FieldLabel: "FEPŻ 2024",
		FieldName:  "source_fepz",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
	"source_short_term": {
		FieldLabel: "Odbiór Krótki Termin",
		FieldName:  "source_short_term",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
	"source_internal_only": {
		FieldLabel: "Tylko nasz magazyn",
		FieldName:  "source_internal_only",
		FiledType:  "text",
		FieldValue: "Nie",
		FieldError: "",
	},
}

var FoodSourcesStepFieldsOrder = []string{
	"source_networks",
	"source_markets",
	"source_machfit",
	"source_fepz",
	"source_short_term",
	"source_internal_only",
}

func GetFoodSourcesStep() *model.OrgStepData {
	return &model.OrgStepData{
		Fields:      FoodSourcesStepFieldsMap,
		FieldsOrder: FoodSourcesStepFieldsOrder,
		Title:   "Źródła żywności",
	}
}
