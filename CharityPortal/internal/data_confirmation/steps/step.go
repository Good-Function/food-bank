package steps

import "charity_portal/internal/data_confirmation/model"

type Step interface {
	GetStepData() *model.OrgStepData
	ValidateStepData() error
}
