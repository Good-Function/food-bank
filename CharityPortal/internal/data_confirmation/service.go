package dataconfirmation

import (
	"charity_portal/internal/data_confirmation/model"
	"charity_portal/internal/data_confirmation/model/validators"
	"charity_portal/internal/data_confirmation/steps"
)

type DataConfirmationService struct {
	Steps map[int]steps.Step
}

func NewDataConfirmationService() *DataConfirmationService {
	orgSteps := make(map[int]steps.Step)
	orgSteps[model.StepOrganizationData] = steps.NewAddressStep()
	orgSteps[model.StepContactData] = steps.NewContactStep()
	orgSteps[model.StepAccountingData] = steps.NewAccountingStep()
	orgSteps[model.StepBeneficiariesData] = steps.NewBeneficiariesStep()
	orgSteps[model.StepFoodSources] = steps.NewFoodSourcesStep()
	orgSteps[model.StepFoodAidConditions] = steps.NewFoodAidConditionsStep()

	return &DataConfirmationService{
		Steps: orgSteps,
	}
}

func (dcs *DataConfirmationService) ValidateStepFieldInput(step int, fieldID, fieldValue string) (*model.FieldInput, error) {
	if step < 0 || step >= len(dcs.Steps) {
		return nil, nil
	}
	stepData, ok := dcs.Steps[step]
	if !ok {
		return nil, nil
	}

	inputType := stepData.GetStepData().Fields[fieldID].FiledType
	var errMsg string
	switch inputType {
	case "text":
		errMsg = validators.IsTextFieldValid(fieldValue, false)
	case "email":
		errMsg = validators.IsEmailFieldValid(fieldValue, false)
	case "tel":
		errMsg = validators.IsTelFieldValid(fieldValue, false)
	case "number":
		errMsg = validators.IsNumberFieldValid(fieldValue, false)
	default:
	}

	return &model.FieldInput{
		FieldLabel: stepData.GetStepData().Fields[fieldID].FieldLabel,
		FieldName:  stepData.GetStepData().Fields[fieldID].FieldName,
		FiledType:  stepData.GetStepData().Fields[fieldID].FiledType,
		FieldValue: fieldValue,
		FieldError: errMsg,
	}, nil
}

func (dcs *DataConfirmationService) GetOrganizationDataFirstStep() (*model.OrganizationDataStep, error) {
	currentStep := 0
	return dcs.FillOrgDataStep(currentStep), nil
}

func (dcs *DataConfirmationService) HandleNextStep(currentStep int) (*model.OrganizationDataStep, error) {
	newCurrentStep := dcs.GetNewCurrentStep(currentStep)
	return dcs.FillOrgDataStep(newCurrentStep), nil
}

func (dcs *DataConfirmationService) HandlePreviousStep(currentStep int) (*model.OrganizationDataStep, error) {
	newCurrentStep := dcs.GetNewPreviousStep(currentStep)
	return dcs.FillOrgDataStep(newCurrentStep), nil
}

func (dcs *DataConfirmationService) FillOrgDataStep(step int) *model.OrganizationDataStep {
	stepData := dcs.Steps[step].GetStepData()
	return &model.OrganizationDataStep{
		CurrentStep:       step,
		CurrentStepTitle:  stepData.Title,
		NextStep:          dcs.GetNewNextStep(step),
		NextStepTitle:     dcs.GetNextStepTitle(step),
		PreviousStep:      dcs.GetNewPreviousStep(step),
		PreviousStepTitle: dcs.GetPreviousStepTitle(step),
		Fields:            stepData.Fields,
		FieldsOrder:       stepData.FieldsOrder,
	}
}

func (dcs *DataConfirmationService) GetNewCurrentStep(currentStep int) int {
	if currentStep+1 < len(dcs.Steps) {
		return currentStep + 1
	}
	return currentStep
}

func (dcs *DataConfirmationService) GetNewNextStep(currentStep int) int {
	if currentStep+1 < len(dcs.Steps) {
		return currentStep + 1
	}
	return currentStep
}

func (dcs *DataConfirmationService) GetNewPreviousStep(currentStep int) int {
	if currentStep-1 >= 0 {
		return currentStep - 1
	}
	return 0
}

func (dcs *DataConfirmationService) GetNextStepTitle(currentStep int) string {
	if currentStep+1 < len(dcs.Steps) {
		return dcs.Steps[currentStep+1].GetStepData().Title
	}
	return ""
}

func (dcs *DataConfirmationService) GetPreviousStepTitle(currentStep int) string {
	if currentStep-1 >= 0 {
		return dcs.Steps[currentStep-1].GetStepData().Title
	}
	return ""
}
