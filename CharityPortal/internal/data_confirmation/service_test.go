package dataconfirmation

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestNewDataConfirmationService_InitializesAllSteps(t *testing.T) {
	service := NewDataConfirmationService()
	assert.Equal(t, 6, len(service.OrganizationData), "Should initialize all 6 steps")
}

func TestShouldReturnProperNextStepCalculatedValues(t *testing.T) {
	testCases := []struct {
		currentStep          int
		expectedCurrentStep  int
		expectedNextStep     int
		expectedPreviousStep int
	}{
		{
			currentStep:          StepOrganizationData,
			expectedCurrentStep:  StepContactData,
			expectedNextStep:     StepAccountingData,
			expectedPreviousStep: StepOrganizationData,
		},
		{
			currentStep:          StepFoodAidConditions,
			expectedCurrentStep:  StepFoodAidConditions,
			expectedNextStep:     StepFoodAidConditions,
			expectedPreviousStep: StepFoodSources,
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
	assert.Equal(t, StepOrganizationData, render.CurrentStep, "Current step should be the first step, returned: %d", render.CurrentStep)
	assert.Equal(t, StepContactData, render.NextStep, "Next step should be the second step, returned: %d", render.NextStep)
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
			currentStep:          StepOrganizationData,
			expectedCurrentStep:  StepOrganizationData,
			expectedNextStep:     StepContactData,
			expectedPreviousStep: StepOrganizationData,
		},
		{
			currentStep:          StepContactData,
			expectedCurrentStep:  StepOrganizationData,
			expectedNextStep:     StepContactData,
			expectedPreviousStep: StepOrganizationData,
		},
		{
			currentStep:          StepFoodAidConditions,
			expectedCurrentStep:  StepFoodSources,
			expectedNextStep:     StepFoodAidConditions,
			expectedPreviousStep: StepBeneficiariesData,
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
