package charityupdate

type Config struct {
	OperatorApiClientId string `toml:"operator_api_client_id"`
	OperatorApiBaseUrl string `toml:"operator_api_base_url"`
	MockOperatorApi bool `toml:"mock_operator_api"`
}