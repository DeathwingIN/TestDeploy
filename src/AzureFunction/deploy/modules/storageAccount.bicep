param conventions object
param resourceNames object
param attachmentsContainerName string

@description('Log Analytics Workspace Id')
param logAnalyticsWorkspaceId string

@description('Configurations based on the environment type.')
var environmentConfigurationMap = {
  dev: {
    sku: 'Standard_LRS'
  }
  uat: {
    sku: 'Standard_LRS' // no need of ZRS for non-prod environment
  }
  prd: {
    sku: 'Standard_LRS' // Standard_LRS is used here to avoid additional cost
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: resourceNames.storageAccount.name
  location: conventions.location
  tags: conventions.baseTags
  kind: 'StorageV2'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: environmentConfigurationMap[conventions.environment].sku
  }
}


resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name    : 'default'
  parent  : storageAccount
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-06-01' = {
  name    : attachmentsContainerName
  parent  : blobService
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2021-06-01' = {
  name: 'default'
  parent: storageAccount
}

resource checkpointTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-06-01' = {
  name:'keepalivecheckpoints' //resourceNames.storageAccount.checkpointTableName
  parent: tableService
}

resource diagnosticSettingsBlob 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'diagnosticSettingsmapBlob'
  scope: blobService
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    storageAccountId: storageAccount.id
    metrics: [
      {
        enabled: true
        category: 'Transaction'
      }
    ]
    logs: [
      {
        enabled: true
        category: 'StorageRead'
      }
      {
        enabled: true
        category: 'StorageWrite'
      }
      {
        enabled: true
        category: 'StorageDelete'
      }
    ]
  }
}

output name string = storageAccount.name
