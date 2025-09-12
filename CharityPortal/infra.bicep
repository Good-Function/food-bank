@description('Name for the app')
param name string 

@description('Location for all resources.')
param location string = resourceGroup().location

@description('ACR Login')
param acrlogin string

@description('ACR endpoint')
param acrendpoint string

@description('Container image to deploy.')
param image string 

@secure()
@description('ACR Password')
param acrpassword string 

@secure()
@description('PostgreSQL charity_user Password')
param dbCharityUserPassword string

@description('PostgreSQL Server Name')
param dbServerName string = 'foodbank-postgres'

@secure()
@description('Client ID for authentication')
param authClientId string

@secure()
@description('Client secret for authentication')
param authClientSecret string

@description('Redirect URL for authentication')
param authRedirectUrl string

@description('Tenant name for authentication')
param authTenantName string

@secure()
@description('Tenant ID for authentication')
param authTenantId string

@secure()
@description('Session hash key')
param authHashKey string

@secure()
@description('Session block key')
param authBlockKey string

@description('OAuth state value')
param authState string

resource managedEnv 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: 'operator-portal'
}

resource charityPortalApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: name
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    managedEnvironmentId: managedEnv.id
    configuration: {
      secrets: [
        {
          name: 'containerregistrypasswordref'
          value: acrpassword
        }
        {
          name: 'operatordbconnectionstringref'
          value: 'host=${dbServerName}.postgres.database.azure.com user=charity_user@foodbank-postgres password=${dbCharityUserPassword} dbname=foodbankdb sslmode=require'
        }
        {
          name: 'authclientidref'
          value: authClientId
        }
        {
          name: 'authclientsecretref'
          value: authClientSecret
        }
        {
          name: 'authtenantidref'
          value: authTenantId
        }
        {
          name: 'authhashkeyref'
          value: authHashKey
        }
        {
          name: 'authblockkeyref'
          value: authBlockKey
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acrendpoint
          username: acrlogin
          passwordSecretRef: 'containerregistrypasswordref'
        }
      ]
    }
    template: {
      containers: [
        {
          image: image
          name: name
          resources: {
            cpu: json('0.75')
            memory: '1.5Gi'
          }
          env: [
            {
              name: 'OPERATOR_DB_CONNECTION_STRING'
              secretRef: 'operatordbconnectionstringref'
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            { 
              name: 'OPERATOR_API_BASE_URL'
              value: 'https://operator-portal.bluemeadow-0985b16b.polandcentral.azurecontainerapps.io'
            }
            {
              name: 'AUTH_CLIENT_ID'
              secretRef: 'authclientidref'
            }
            {
              name: 'AUTH_CLIENT_SECRET'
              secretRef: 'authclientsecretref'
            }
            {
              name: 'AUTH_TENANT_ID'
              secretRef: 'authtenantidref'
            }
            {
              name: 'AUTH_HASH_KEY'
              secretRef: 'authhashkeyref'
            }
            {
              name: 'AUTH_BLOCK_KEY'
              secretRef: 'authblockkeyref'
            }
            {
              name: 'AUTH_REDIRECT_URL'
              value: authRedirectUrl
            }
            {
              name: 'AUTH_TENANT_NAME'
              value: authTenantName
            }
            {
              name: 'AUTH_STATE'
              value: authState
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

output fqdn string = charityPortalApp.properties.configuration.ingress.fqdn
