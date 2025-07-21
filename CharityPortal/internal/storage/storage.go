package storage

type Storage interface {
	ReadOrganizationByEmail(email string) (*OrganizationDTO, error)
}
