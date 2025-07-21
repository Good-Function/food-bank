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

type OrganizationDataStep struct {
	CurrentStep       int                    `json:"current_step"`
	CurrentStepTitle  string                 `json:"current_step_title"`
	Fields            map[string]*FieldInput `json:"fields"`
	FieldsOrder       []string               `json:"fields_order"`
	NextStep          int                    `json:"next_step"`
	NextStepTitle     string                 `json:"next_step_title"`
	PreviousStep      int                    `json:"previous_step"`
	PreviousStepTitle string                 `json:"previous_step_title"`
}

type OrgStepData struct {
	Title       string                 `json:"title"`
	Fields      map[string]*FieldInput `json:"fields"`
	FieldsOrder []string               `json:"fields_order"`
}

type FieldInput struct {
	FieldLabel         string   `json:"field_label"`
	FieldName          string   `json:"field_name"`
	FiledType          string   `json:"field_type"`
	FieldValue         string   `json:"field_value"`
	FieldError         string   `json:"field_error"`
	SelectFieldOptions []string `json:"select_field_options"`
}
