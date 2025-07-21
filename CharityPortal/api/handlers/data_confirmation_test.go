package handlers

import (
	dataconfirmation "charity_portal/internal/data_confirmation"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"github.com/PuerkitoBio/goquery"
	"github.com/stretchr/testify/assert"
)

func TestShouldReturnFirstStepDataConfirmationRequestOnInitialCall(t *testing.T) {
	dataConfirmationService := dataconfirmation.NewDataConfirmationService()
	handler := NewDataConfirmationHandler(dataConfirmationService)
	req := httptest.NewRequest("POST", "/data-confirmation", nil)
	rr := httptest.NewRecorder()
	handler.ServeHTTP(rr, req)
	assert.Equal(t, http.StatusOK, rr.Code, "Expected status code 200 for data confirmation request")
	returnedHtml := rr.Body.String()

	doc, err := goquery.NewDocumentFromReader(strings.NewReader(returnedHtml))
	assert.NoError(t, err, "Should not return an error when parsing HTML")
	assert.NotNil(t, doc, "Parsed document should not be nil")

	assert.Equal(t, "Dane adresowe", doc.Find("#data-confirmation-step-name").Text(), "First step title should be 'Dane adresowe'")
	assert.Equal(t, 1, doc.Find(`button[name="step_action"][value="next"]`).Length(), "Should have one 'next' button for the first step")
}

func TestDataConfirmationStepNavigation(t *testing.T) {
	dataConfirmationService := dataconfirmation.NewDataConfirmationService()
	handler := NewDataConfirmationHandler(dataConfirmationService)

	type testCase struct {
		name               string
		currentStep        string
		stepAction         string
		expectedStepTitle  string
		expectedNextButton string
		expectedPrevButton string
	}

	testCases := []testCase{
		{
			name:               "Go to next step from first",
			currentStep:        "0",
			stepAction:         "next",
			expectedStepTitle:  "Dane kontaktowe",
			expectedNextButton: "Księgowość",
			expectedPrevButton: "Dane adresowe",
		},
		{
			name:               "Go to previous step from second",
			currentStep:        "1",
			stepAction:         "prev",
			expectedStepTitle:  "Dane adresowe",
			expectedNextButton: "Dane kontaktowe",
			expectedPrevButton: "",
		},
		{
			name:               "Render save button on last step",
			currentStep:        "5",
			stepAction:         "next",
			expectedStepTitle:  "Warunki udzielania pomocy żywnościowej",
			expectedNextButton: "",
			expectedPrevButton: "Źródła żywności",
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Initial request to simulate context
			req := httptest.NewRequest("POST", "/data-confirmation", nil)
			rr := httptest.NewRecorder()
			handler.ServeHTTP(rr, req)
			assert.Equal(t, http.StatusOK, rr.Code, "Expected status 200 on initial load")

			// Perform actual test request with form values
			req = httptest.NewRequest("POST", "/data-confirmation", nil)
			req.Form = map[string][]string{
				"current_step": {tc.currentStep},
				"step_action":  {tc.stepAction},
			}
			rr = httptest.NewRecorder()
			handler.ServeHTTP(rr, req)

			assert.Equal(t, http.StatusOK, rr.Code, "Expected status 200 for step transition")

			doc, err := goquery.NewDocumentFromReader(rr.Body)
			assert.NoError(t, err)
			assert.NotNil(t, doc)

			assert.Equal(t, tc.expectedStepTitle, doc.Find("#data-confirmation-step-name").Text())
			nextButton := doc.Find(`button[name="step_action"][value="next"]`)
			prevButton := doc.Find(`button[name="step_action"][value="prev"]`)
			if tc.expectedNextButton == "" {
				assert.Equal(t, 0, nextButton.Length(), "Expected no 'next' button on this step")
			} else {
				assert.Equal(t, 1, nextButton.Length(), "Expected one 'next' button on this step")
				assert.Equal(t, tc.expectedNextButton, nextButton.Text(), "Next button text should match expected")
			}
			if tc.expectedPrevButton == "" {
				assert.Equal(t, 0, prevButton.Length(), "Expected no 'prev' button on this step")
			} else {
				assert.Equal(t, 1, prevButton.Length(), "Expected one 'prev' button on this step")
				assert.Equal(t, tc.expectedPrevButton, prevButton.Text(), "Previous button text should match expected")
			}
		})
	}
}

func TestDataConfirmationStepFieldValidation(t *testing.T) {
	dataConfirmationService := dataconfirmation.NewDataConfirmationService()
	handler := NewDataConfirmationHandler(dataConfirmationService)

	type testCase struct {
		name              string
		step              string
		triggerField      string
		inputName         string
		inputValue        string
		expectedStatus    int
		expectedErrorText string
	}

	testCases := []testCase{
		{
			name:           "Valid organization input on step 0",
			step:           "0",
			triggerField:   "organization",
			inputName:      "organization",
			inputValue:     "Test Organization",
			expectedStatus: http.StatusOK,
		},
		{
			name:              "Invalid phone input on step 1",
			step:              "1",
			triggerField:      "phone",
			inputName:         "phone",
			inputValue:        "saf",
			expectedStatus:    http.StatusOK,
			expectedErrorText: "Numer telefonu jest nieprawidłowy.",
		},
		{
			name:           "Valid phone input on step 1",
			step:           "1",
			triggerField:   "phone",
			inputName:      "phone",
			inputValue:     "+48 123 456 789",
			expectedStatus: http.StatusOK,
		},
		{
			name:              "Invalid email input on step 1",
			step:              "1",
			triggerField:      "email",
			inputName:         "email",
			inputValue:        "saf",
			expectedStatus:    http.StatusOK,
			expectedErrorText: "Adres e-mail jest nieprawidłowy.",
		},
		{
			name:           "Valid email input on step 1",
			step:           "1",
			triggerField:   "email",
			inputName:      "email",
			inputValue:     "test@example.com",
			expectedStatus: http.StatusOK,
		},
		// Add more cases here as needed
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			req := httptest.NewRequest("POST", "/data-confirmation", nil)
			req.Header.Set("Hx-Trigger-Name", tc.triggerField)
			req.Form = map[string][]string{
				"current_step": {tc.step},
				"step_action":  {"validate"},
				tc.inputName:   {tc.inputValue},
			}
			rr := httptest.NewRecorder()

			handler.ServeHTTP(rr, req)

			assert.Equal(t, tc.expectedStatus, rr.Code)

			if tc.expectedErrorText != "" {
				doc, err := goquery.NewDocumentFromReader(rr.Body)
				assert.NoError(t, err)
				assert.NotNil(t, doc)

				errorText := doc.Find("#fieldError").Text()
				assert.Equal(t, tc.expectedErrorText, errorText, "Expected error message for field to match")
			}
		})
	}
}

func TestDataConfirmationAbandonAction(t *testing.T) {
	dataConfirmationService := dataconfirmation.NewDataConfirmationService()
	handler := NewDataConfirmationHandler(dataConfirmationService)

	req := httptest.NewRequest("POST", "/data-confirmation", nil)
	req.Form = map[string][]string{
		"current_step": {"0"},
		"step_action":  {"abandon"},
	}
	rr := httptest.NewRecorder()

	handler.ServeHTTP(rr, req)

	assert.Equal(t, http.StatusOK, rr.Code, "Expected status code 200 for abandon action")
	assert.Contains(t, rr.Body.String(), "Uzupełnij dane organizacji", "Should redirect to dashboard after abandoning data confirmation")
}
