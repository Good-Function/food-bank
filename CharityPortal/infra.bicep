param name string = 'charityportal'
param image string
param location string = resourceGroup().location
param acrendpoint string
param acrlogin string
@secure() param acrpassword string
@secure() param dbAdminPassword string
param dbServerName string
param dbName string
param storageAccountName string

resource charityPortalApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: environment.id
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
        {
          name: 'blobstorageconnectionref'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
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
