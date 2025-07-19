package handlers

import (
	"charity_portal/internal/data_confirmation"
	"fmt"
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
	fmt.Println(returnedHtml)

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
		name                string
		currentStep         string
		stepAction          string
		expectedStepTitle   string
		expectedButtonValue string
	}

	testCases := []testCase{
		{
			name:                "Go to next step from first",
			currentStep:         "0",
			stepAction:          "next",
			expectedStepTitle:   "Dane kontaktowe",
			expectedButtonValue: "next",
		},
		{
			name:                "Go to previous step from second",
			currentStep:         "1",
			stepAction:          "prev",
			expectedStepTitle:   "Dane adresowe",
			expectedButtonValue: "next",
		},
		{
			name:                "Render save button on last step",
			currentStep:         "5",
			stepAction:          "next",
			expectedStepTitle:   "Warunki udzielania pomocy żywnościowej",
			expectedButtonValue: "save",
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
			assert.Equal(t, 1, doc.Find(`button[name="step_action"][value="`+tc.expectedButtonValue+`"]`).Length())
		})
	}
}
