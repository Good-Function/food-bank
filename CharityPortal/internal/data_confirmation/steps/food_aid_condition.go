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

func GetFoodAidConditionsStep() *model.OrgStepData {
	return &model.OrgStepData{
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
