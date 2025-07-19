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

func GetFoodSourcesStep() *model.OrgStepData {
	return &model.OrgStepData{
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
