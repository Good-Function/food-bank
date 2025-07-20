package validators

func IsTextFieldValid(input string, required bool) (bool, string) {
	if required && input == "" {
		return false, "Pole tekstowe jest wymagane."
	}
	if len(input) > 255 {
		return false, "Pole tekstowe nie mogą przekraczać 255 znaków."
	}
	return true, ""
}
