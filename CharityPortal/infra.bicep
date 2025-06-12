@description('Name for the container group')
// TODO: Fix - this is correct, but it is called form GH actions with operator-portal. Rename it to appName, this could be used for bot image name and az container app name.
param name string = 'foodbank'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('ACR Login')
param acrlogin string

@description('ACR Login')
param acrendpoint string

@description('Container image to deploy.')
param image string 

@secure()
@description('ACR Password')
param acrpassword string 

@secure()
@description('PostgreSQL Admin Password')
param dbAdminPassword string

@description('PostgreSQL Server Name')
param dbServerName string = 'foodbank-postgres'

@description('PostgreSQL Database Name')
param dbName string = 'foodbankdb'

resource managedEnv 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: 'operator-portal' // TODO fix later
}


resource charityPortalApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: managedEnv.id
    configuration: {
      secrets: [
        {
          name: 'containerregistrypasswordref'
          value: acrpassword
        }
        {
          name: 'dbconnectionstringref'
          value: 'Host=${dbServerName}.postgres.database.azure.com;Database=${dbName};Username=pgadmin;Password=${dbAdminPassword};SslMode=Require;'
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
              name: 'DbConnectionString'
              secretRef: 'dbconnectionstringref'
            }
            {
              name: 'BlobStorageConnectionString'
              secretRef: 'blobstorageconnectionref'
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
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
