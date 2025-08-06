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

var BeneficiariesStepFieldsMap = map[string]*model.FieldInput{
	"beneficiary_count": {
		FieldLabel: "Liczba Beneficjentów",
		FieldName:  "beneficiary_count",
		FiledType:  "number",
		FieldValue: "100",
		FieldError: "",
	},
	"beneficiary_group": {
		FieldLabel: "Beneficjenci",
		FieldName:  "beneficiary_group",
		FiledType:  "text",
		FieldValue: "seniorzy",
		FieldError: "",
	},
}

var BeneficiariesStepFieldsOrder = []string{
	"beneficiary_count",
	"beneficiary_group",
}

func GetBeneficiariesDataStep() *model.OrgStepData {
	return &model.OrgStepData{
		Title:       "Beneficjenci",
		Fields:      BeneficiariesStepFieldsMap,
		FieldsOrder: BeneficiariesStepFieldsOrder,
	}
}
