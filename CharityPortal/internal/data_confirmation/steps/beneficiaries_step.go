package steps

import "charity_portal/internal/data_confirmation/model"

type BeneficiariesStep struct{}

func (s *BeneficiariesStep) GetStepData() *model.OrgStepData {
	return GetBeneficiariesDataStep()
}

func (s *BeneficiariesStep) ValidateStepData() error {
	// Implement validation logic if needed
	return nil
}

func NewBeneficiariesStep() *BeneficiariesStep {
	return &BeneficiariesStep{}
}

func GetBeneficiariesDataStep() *model.OrgStepData{
	return &model.OrgStepData{
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
