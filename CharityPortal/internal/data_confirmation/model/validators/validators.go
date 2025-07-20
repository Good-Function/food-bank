package validators

func IsTextFieldValid(input string, required bool) string {
	if required && input == "" {
		return "Pole tekstowe jest wymagane."
	}
	if len(input) > 255 {
		return "Pole tekstowe nie mogą przekraczać 255 znaków."
	}
	return ""
}
