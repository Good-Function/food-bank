package steps

import "charity_portal/internal/data_confirmation/model"

type ContactStep struct{}

func (s *ContactStep) GetStepData() *model.OrgStepData {
	return GetContactDataStep()
}

func (s *ContactStep) ValidateStepData() error {
	// Implement validation logic if needed
	return nil
}

func NewContactStep() *ContactStep {
	return &ContactStep{}
}

func GetContactDataStep() *model.OrgStepData {
	return &model.OrgStepData{
		Title: "Dane kontaktowe",
		Labels: []string{
			"www / facebook",
			"Telefon",
			"Przedstawiciel",
			"Kontakt",
			"E-mail",
			"Dostępność",
			"Osoba do kontaktu",
			"Tel. osoby kontaktowej",
			"E-mail do osoby kontaktowej",
			"Osoba odbierająca żywność",
			"Telefon do os. odbierającej",
		},
		Types: []string{
			"text",
			"text",
			"text",
			"text",
			"email",
			"text",
			"text",
			"text",
			"email",
			"text",
			"text",
		},
		Names: []string{
			"website",
			"phone",
			"representative",
			"contact_name",
			"email",
			"availability",
			"contact_person",
			"contact_phone",
			"contact_email",
			"food_receiver",
			"food_receiver_phone",
		},
		Values: []string{
			"www.emeryci.michalowice.pl",
			"22 723 93 55",
			"",
			"",
			"kolo1kom@o2.pl",
			"wt 9:00-12:00, czw 14:00-16:00",
			"Przewodniczący Henryk Rybkowski",
			"602231649",
			"",
			"Wiesław Tuka",
			"514517900",
		},
	}
}
