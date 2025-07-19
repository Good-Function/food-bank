package handlers

import (
	"charity_portal/internal/data_confirmation"
	"charity_portal/internal/data_confirmation/model"
	"charity_portal/web/components"
	"net/http"
	"strconv"
)

type DataConfirmationHandler struct {
	dataConfirmationService *dataconfirmation.DataConfirmationService
}

func NewDataConfirmationHandler(dataConfirmationService *dataconfirmation.DataConfirmationService) *DataConfirmationHandler {
	return &DataConfirmationHandler{
		dataConfirmationService: dataConfirmationService,
	}
}

func (dch *DataConfirmationHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	if err := r.ParseForm(); err != nil {
		http.Error(w, "Failed to parse form", http.StatusBadRequest)
		return
	}

	currentStepString := r.FormValue("current_step")
	currentStep, err := strconv.Atoi(currentStepString)
	if err != nil {
		currentStep = 0
	}

	var renderData *model.OrganizationDataRender

	stepAction := r.FormValue("step_action")
	switch stepAction {
	case model.ACTION_NEXT:
		renderData, _ = dch.dataConfirmationService.HandleNextStep(currentStep)
	case model.ACTION_PREVIOUS:
		renderData, _ = dch.dataConfirmationService.HandlePreviousStep(currentStep)
	case model.ACTION_SAVE:
	case model.ACTION_ABANDON:
	default:
		if currentStep != 0 {
			http.Error(w, "Invalid step action", http.StatusBadRequest)
			return
		}
		renderData, _ = dch.dataConfirmationService.GetOrganizationDataFirstStep()
	}

	components.DataConfirmation(*renderData).Render(r.Context(), w)
}
