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

var ContactStepFieldsMap = map[string]*model.FieldInput{
	"website": {
		FieldLabel: "www / facebook",
		FieldName:  "website",
		FiledType:  "text",
		FieldValue: "www.google.warszawa.pl",
		FieldError: "",
	},
	"phone": {
		FieldLabel: "Telefon",
		FieldName:  "phone",
		FiledType:  "tel",
		FieldValue: "22 722 43 34",
		FieldError: "",
	},
	"representative": {
		FieldLabel: "Przedstawiciel",
		FieldName:  "representative",
		FiledType:  "text",
		FieldValue: "",
		FieldError: "",
	},
	"contact_name": {
		FieldLabel: "Kontakt",
		FieldName:  "contact_name",
		FiledType:  "text",
		FieldValue: "",
		FieldError: "",
	},
	"email": {
		FieldLabel: "E-mail",
		FieldName:  "email",
		FiledType:  "email",
		FieldValue: "fsafasdf@o2.pl",
		FieldError: "",
	},
	"availability": {
		FieldLabel: "Dostępność",
		FieldName:  "availability",
		FiledType:  "text",
		FieldValue: "wt 9:00-12:00, czw 14:00-16:00",
		FieldError: "",
	},
	"contact_person": {
		FieldLabel: "Osoba do kontaktu",
		FieldName:  "contact_person",
		FiledType:  "text",
		FieldValue: "Anonimowy",
		FieldError: "",
	},
	"contact_phone": {
		FieldLabel: "Tel. osoby kontaktowej",
		FieldName:  "contact_phone",
		FiledType:  "text",
		FieldValue: "123 123 123",
		FieldError: "",
	},
	"contact_email": {
		FieldLabel: "E-mail do osoby kontaktowej",
		FieldName:  "contact_email",
		FiledType:  "email",
		FieldValue: "",
		FieldError: "",
	},
	"food_receiver": {
		FieldLabel: "Osoba odbierająca żywność",
		FieldName:  "food_receiver",
		FiledType:  "text",
		FieldValue: "anonimowy anonim",
		FieldError: "",
	},
	"food_receiver_phone": {
		FieldLabel: "Telefon do os. odbierającej",
		FieldName:  "food_receiver_phone",
		FiledType:  "text",
		FieldValue: "123 123 123",
		FieldError: "",
	},
}

var ContactStepFieldsOrder = []string{
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
}

func GetContactDataStep() *model.OrgStepData {
	return &model.OrgStepData{
		Title:   "Dane kontaktowe",
		Fields:      ContactStepFieldsMap,
		FieldsOrder: ContactStepFieldsOrder,
	}
}
