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

@secure()
@description('Microsoft Entra - App Registration client secret')
param msEntraClientSecret string

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

@secure()
@description('Password for charity user')
param dbCharityPassword string

@description('Storage Account Name')
param storageAccountName string = 'storage0${uniqueString(resourceGroup().id)}'

@description('Blob Container Name')
param blobContainerName string = 'uploads'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/${blobContainerName}'
  properties: {
    publicAccess: 'None'
  }
}

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
          addressPrefix: '10.0.0.0/23'
          delegations: [
            {
              name: 'Microsoft.DBforPostgreSQL'
              properties: {
                serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
              }
            }
          ]
        }
      }
      {
        name: subnetAppName
        properties: {
          addressPrefix: '10.0.2.0/23'
        }
      }
    ]
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
    name: 'privatelink.postgres.database.azure.com'
    location: 'global'
    resource vNetLink 'virtualNetworkLinks' = {
        name: 'privatelink.postgres.database.azure.com'
        location: 'global'
        properties: {
            registrationEnabled: false
            virtualNetwork: { id: vnet.id }
        }
    }
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-11-01-preview' = {
  name: dbServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
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
  resource postgresConfig 'configurations' = {
      name: 'azure.extensions'
      properties: {
        value: 'pg_trgm'
        source: 'user-override'
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

resource createReadonlyUser 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'createCharityUser'
  location: location
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.37.0'
    retentionInterval: 'P1D'
    environmentVariables: [
      {
        name: 'PGPASSWORD'
        secureValue: dbAdminPassword
      }
      {
        name: 'CHARITY_PASSWORD'
        secureValue: dbCharityPassword
      }
    ]
    
    scriptContent: '''
      echo "Creating charityUser user if not exists..."
      psql "host=${dbServerName}.postgres.database.azure.com port=5432 dbname=${dbName} user=pgadmin@${dbServerName} sslmode=require" \
       -v CHARITY_PASSWORD="'$CHARITY_PASSWORD'" <<'EOF'
      DO
      $do$
      BEGIN
        IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'charity_user') THEN
          CREATE USER charity_user WITH PASSWORD :'CHARITY_PASSWORD';
        END IF;
      END
      $do$;

      GRANT CONNECT ON DATABASE ${dbName} TO charity_user;
      GRANT USAGE ON SCHEMA public TO charity_user;
      GRANT SELECT ON TABLE organizacje TO charity_user;
      EOF
    '''
  }
  dependsOn: [
    database
  ]
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
    vnetConfiguration: {
      infrastructureSubnetId: vnet.properties.subnets[1].id  // Ensure this is the correct subnet
    }
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
          name: 'msentraclientsecretref'
          value: msEntraClientSecret
        }
        {
          name: 'dbconnectionstringref'
          value: 'Host=${dbServerName}.postgres.database.azure.com;Database=${dbName};Username=pgadmin;Password=${dbAdminPassword};SslMode=Require;'
        }
        {
          name: 'blobstorageconnectionref'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
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
              name: 'AzureAd__ClientSecret'
              secretRef: 'msentraclientsecretref'
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

output fqdn string = foodbankapp.properties.configuration.ingress.fqdn
