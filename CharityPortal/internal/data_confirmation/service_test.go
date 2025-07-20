package dataconfirmation

import (
	"charity_portal/internal/data_confirmation/model"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestShouldReturnProperNextStepCalculatedValues(t *testing.T) {
	testCases := []struct {
		currentStep          int
		expectedCurrentStep  int
		expectedNextStep     int
		expectedPreviousStep int
	}{
		{
			currentStep:          model.StepOrganizationData,
			expectedCurrentStep:  model.StepContactData,
			expectedNextStep:     model.StepAccountingData,
			expectedPreviousStep: model.StepOrganizationData,
		},
		{
			currentStep:          model.StepFoodAidConditions,
			expectedCurrentStep:  model.StepFoodAidConditions,
			expectedNextStep:     model.StepFoodAidConditions,
			expectedPreviousStep: model.StepFoodSources,
		},
	}
	for _, tc := range testCases {
		service := NewDataConfirmationService()
		render, err := service.HandleNextStep(tc.currentStep)
		assert.NoError(t, err, "Should not return an error")
		assert.Equal(t, tc.expectedCurrentStep, render.CurrentStep, "Current step should match expected value")
		assert.Equal(t, tc.expectedNextStep, render.NextStep, "Next step should match expected value")
		assert.Equal(t, tc.expectedPreviousStep, render.PreviousStep, "Previous step should match expected value")
	}
}

func TestShouldReturnFirstStepOnInitialization(t *testing.T) {
	service := NewDataConfirmationService()
	render, err := service.GetOrganizationDataFirstStep()
	assert.NoError(t, err, "Should not return an error")
	assert.Equal(t, model.StepOrganizationData, render.CurrentStep, "Current step should be the first step, returned: %d", render.CurrentStep)
	assert.Equal(t, model.StepContactData, render.NextStep, "Next step should be the second step, returned: %d", render.NextStep)
	assert.Equal(t, 0, render.PreviousStep, "Previous step should be zero for the first step, returned: %d", render.PreviousStep)
}

func TestShouldReturnPreviousStepOnHandlePreviousStep(t *testing.T) {
	testCases := []struct {
		currentStep          int
		expectedCurrentStep  int
		expectedNextStep     int
		expectedPreviousStep int
	}{
		{
			currentStep:          model.StepOrganizationData,
			expectedCurrentStep:  model.StepOrganizationData,
			expectedNextStep:     model.StepContactData,
			expectedPreviousStep: model.StepOrganizationData,
		},
		{
			currentStep:          model.StepContactData,
			expectedCurrentStep:  model.StepOrganizationData,
			expectedNextStep:     model.StepContactData,
			expectedPreviousStep: model.StepOrganizationData,
		},
		{
			currentStep:          model.StepFoodAidConditions,
			expectedCurrentStep:  model.StepFoodSources,
			expectedNextStep:     model.StepFoodAidConditions,
			expectedPreviousStep: model.StepBeneficiariesData,
		},
	}
	for _, tc := range testCases {
		service := NewDataConfirmationService()
		render, err := service.HandlePreviousStep(tc.currentStep)
		assert.NoError(t, err, "Should not return an error")
		assert.Equal(t, tc.expectedCurrentStep, render.CurrentStep, "Current step should match expected value")
		assert.Equal(t, tc.expectedNextStep, render.NextStep, "Next step should match expected value")
		assert.Equal(t, tc.expectedPreviousStep, render.PreviousStep, "Previous step should match expected value")
	}
}
