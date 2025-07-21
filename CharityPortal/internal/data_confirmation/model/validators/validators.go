package validators

import "regexp"

func IsTextFieldValid(input string, required bool) string {
	if required && input == "" {
		return "Pole tekstowe jest wymagane."
	}
	if len(input) > 255 {
		return "Pole tekstowe nie mogą przekraczać 255 znaków."
	}
	return ""
}

func IsEmailFieldValid(email string, required bool) string {
	if required && email == "" {
		return "Adres e-mail jest wymagany."
	}
	if email == "" {
		return ""
	}
	regex := `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$`
	if !regexp.MustCompile(regex).MatchString(email) {
		return "Adres e-mail jest nieprawidłowy."
	}
	return ""
}

func IsTelFieldValid(tel string, required bool) string {
	if required && tel == "" {
		return "Numer telefonu jest wymagany."
	}
	if tel == "" {
		return ""
	}
	regex := `^\+?[0-9\s\-]{9,15}$`
	if !regexp.MustCompile(regex).MatchString(tel) {
		return "Numer telefonu jest nieprawidłowy."
	}
	return ""
}

func IsNumberFieldValid(number string, required bool) string {
	if required && number == "" {
		return "Pole numeryczne jest wymagane."
	}
	if number == "" {
		return ""
	}
	regex := `^\d+([.,]\d{1,2})?$`
	if !regexp.MustCompile(regex).MatchString(number) {
		return "Pole numeryczne jest nieprawidłowe."
	}
	return ""
}
