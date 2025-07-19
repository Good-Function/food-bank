package dataconfirmation

type DataConfirmationService struct {
	OrganizationData []*OrgStepData
}

func NewDataConfirmationService() *DataConfirmationService {
	return &DataConfirmationService{
		OrganizationData: []*OrgStepData{
			GetAddressDataStep(),
			GetContactDataStep(),
			GetAccountingDataStep(),
			GetBeneficiariesDataStep(),
			GetFoodSourcesStep(),
			GetFoodAidConditionsStep(),
		},
	}
}

const (
	StepOrganizationData = iota
	StepContactData
	StepAccountingData
	StepBeneficiariesData
	StepFoodSources
	StepFoodAidConditions
)

const (
	ACTION_NEXT     = "next"
	ACTION_PREVIOUS = "prev"
	ACTION_SAVE     = "save"
	ACTION_ABANDON  = "abandon"
)

func (dcs *DataConfirmationService) GetOrganizationDataFirstStep() (*OrganizationDataRender, error) {
	return &OrganizationDataRender{
		CurrentStep:          0,
		NextStep:             1,
		NextStepTitle:        dcs.GetNextStepTitle(0),
		PreviousStep:         0,
		PreviousStepTitle:    "",
		StepOrganizationData: dcs.OrganizationData,
	}, nil
}

func (dcs *DataConfirmationService) HandleNextStep(currentStep int) (*OrganizationDataRender, error) {
	newCurrentStep := dcs.GetNewCurrentStep(currentStep)
	return &OrganizationDataRender{
		CurrentStep:          newCurrentStep,
		NextStep:             dcs.GetNewNextStep(newCurrentStep),
		NextStepTitle:        dcs.GetNextStepTitle(newCurrentStep),
		PreviousStep:         dcs.GetNewPreviousStep(newCurrentStep),
		PreviousStepTitle:    dcs.GetPreviousStepTitle(newCurrentStep),
		StepOrganizationData: dcs.OrganizationData,
	}, nil
}

func (dcs *DataConfirmationService) HandlePreviousStep(currentStep int) (*OrganizationDataRender, error) {
	newCurrentStep := dcs.GetNewPreviousStep(currentStep)
	return &OrganizationDataRender{
		CurrentStep:          newCurrentStep,
		NextStep:             dcs.GetNewNextStep(newCurrentStep),
		NextStepTitle:        dcs.GetNextStepTitle(newCurrentStep),
		PreviousStep:         dcs.GetNewPreviousStep(newCurrentStep),
		PreviousStepTitle:    dcs.GetPreviousStepTitle(newCurrentStep),
		StepOrganizationData: dcs.OrganizationData,
	}, nil
}

func (dcs *DataConfirmationService) GetNewCurrentStep(currentStep int) int {
	if currentStep+1 < len(dcs.OrganizationData) {
		return currentStep + 1
	}
	return currentStep
}

func (dcs *DataConfirmationService) GetNewNextStep(currentStep int) int {
	if currentStep+1 < len(dcs.OrganizationData) {
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
	if currentStep+1 < len(dcs.OrganizationData) {
		return dcs.OrganizationData[currentStep+1].Title
	}
	return ""
}

func (dcs *DataConfirmationService) GetPreviousStepTitle(currentStep int) string {
	if currentStep-1 >= 0 {
		return dcs.OrganizationData[currentStep-1].Title
	}
	return ""
}
