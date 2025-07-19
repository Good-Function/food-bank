package model

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
	ACTION_VALIDATE = "validate"
)

type OrganizationDataRender struct {
	CurrentStep          int            `json:"current_step"`
	NextStep             int            `json:"next_step"`
	NextStepTitle        string         `json:"next_step_title"`
	PreviousStep         int            `json:"previous_step"`
	PreviousStepTitle    string         `json:"previous_step_title"`
	StepOrganizationData []*OrgStepData `json:"step_organization_data"`
}

type OrgStepData struct {
	Title  string   `json:"title"`
	Labels []string `json:"labels"`
	Types  []string `json:"types"`
	Names  []string `json:"names"`
	Values []string `json:"values"`
}
