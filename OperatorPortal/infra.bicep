@description('Name for the container group')
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

@description('VNET Name')
param vnetName string = 'foodbank-vnet'

@description('Subnet Name for PostgreSQL')
param subnetDbName string = 'postgres-subnet'

@description('Subnet Name for Container Apps')
param subnetAppName string = 'containerapp-subnet'

@secure()
@description('PostgreSQL Admin Password')
param dbAdminPassword string

@description('PostgreSQL Server Name')
param dbServerName string = 'foodbank-postgres'

@description('PostgreSQL Database Name')
param dbName string = 'foodbankdb'

resource vnet 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetDbName
        properties: {
          addressPrefix: '10.0.1.0/24'
          delegations: [
            {
              name: 'Microsoft.DBforPostgreSQL'
              properties: {
                serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
              }
            }
          ]
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
      {
        name: subnetAppName
        properties: {
          addressPrefix: '10.0.2.0/24'
          delegations: [
            {
              name: 'Microsoft.App'
              properties: {
                serviceName: 'Microsoft.App/environments'
              }
            }
          ]
        }
      }
    ]
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: 'privatelink.postgres.database.azure.com'
  location: 'global'
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-11-01-preview' = {
  name: dbServerName
  location: location
  sku: {
    name: 'Standard_D2s_v3'
    tier: 'GeneralPurpose'
  }
  properties: {
    administratorLogin: 'pgadmin'
    administratorLoginPassword: dbAdminPassword
    version: '16'
    network: {
      delegatedSubnetResourceId: vnet.properties.subnets[0].id
      privateDnsZoneArmResourceId: privateDnsZone.id
    }
    storage: {
      storageSizeGB: 32
    }
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-11-01-preview' = {
  parent: postgres
  name: dbName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource law 'Microsoft.OperationalInsights/workspaces@2020-03-01-preview' = {
  name: name
  location: location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource environment 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: name
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: law.properties.customerId
        sharedKey: law.listKeys().primarySharedKey
      }
    }
  }
}

resource foodbankapp 'Microsoft.App/containerApps@2022-03-01' = {
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
          value: 'Host=${dbServerName}.${privateDnsZone.name};Database=${dbName};Username=pgadmin;Password=${dbAdminPassword};SslMode=Require;'
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          // server is in the format of myregistry.azurecr.io
          server: acrendpoint
          username: acrlogin
          passwordSecretRef: 'containerregistrypasswordref'
        }
      ]
    }
    template: {
      containers: [
        {
          // This is in the format of myregistry.azurecr.io
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

output fqdn string = foodbankapp.properties.configuration.ingress.fqdn
