package validators

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestTextFieldValidator(t *testing.T) {
	tests := []struct {
		input    string
		required bool
		expected string
	}{
		{"", true, "Pole tekstowe jest wymagane."},
		{"", false, ""},
		{"a", true, ""},
		{"a", false, ""},
		{"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", true, "Pole tekstowe nie mogą przekraczać 255 znaków."},
	}

	for _, test := range tests {
		result := IsTextFieldValid(test.input, test.required)
		if result != test.expected {
			assert.Equal(t, test.expected, result, "IsTextFieldValid(%q, %t) = %q; expected %q", test.input, test.required, result, test.expected)
		}
	}
}

func TestEmailValidator(t *testing.T) {
	tests := []struct {
		input    string
		required bool
		expected string
	}{
		{"", true, "Adres e-mail jest wymagany."},
		{"", false, ""},
		{"test@example.com", true, ""},
		{"test+123@example.com", true, ""},
	}
	for _, test := range tests {
		result := IsEmailFieldValid(test.input, test.required)
		if result != test.expected {
			assert.Equal(t, test.expected, result, "IsEmailValid(%q, %t) = %q; expected %q", test.input, test.required, result, test.expected)
		}
	}
}

func TestTelFieldValidator(t *testing.T) {
	tests := []struct {
		input    string
		required bool
		expected string
	}{
		{"", true, "Numer telefonu jest wymagany."},
		{"", false, ""},
		{"+48 123 456 789", true, ""},
		{"123 456 789", true, ""},
		{"722 32 23", true, ""},
		{"123-456-7890", true, ""},
		{"1234567890", true, ""},
		{"12345", true, "Numer telefonu jest nieprawidłowy."},
	}

	for _, test := range tests {
		result := IsTelFieldValid(test.input, test.required)
		if result != test.expected {
			assert.Equal(t, test.expected, result, "IsTelFieldValid(%q, %t) = %q; expected %q", test.input, test.required, result, test.expected)
		}
	}
}

func TestNumberFieldValidator(t *testing.T) {
	tests := []struct {
		input    string
		required bool
		expected string
	}{
		{"", true, "Pole numeryczne jest wymagane."},
		{"", false, ""},
		{"123", true, ""},
		{"0", true, ""},
		{"123.45", true, ""},
		{"123,45", true, ""},
		{"0.45", true, ""},
		{"abc", true, "Pole numeryczne jest nieprawidłowe."},
	}

	for _, test := range tests {
		result := IsNumberFieldValid(test.input, test.required)
		if result != test.expected {
			assert.Equal(t, test.expected, result, "IsNumberFieldValid(%q, %t) = %q; expected %q", test.input, test.required, result, test.expected)
		}
	}
}
