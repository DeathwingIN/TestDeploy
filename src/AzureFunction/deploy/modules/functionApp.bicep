param conventions object
param resourceNames object

@description('App Service Plan (Id).')
param appServicePlanId string

@description('Generated storage account name')
param storageAccountName string

@description('Generated storage account name')
param attachmentsContainerName string

@description('AAD Tenant ID')
param aadTenantId string

@description('ISV Client ID')
param isvClientId string

param logAnalyticsWorkspaceId string
param appInsightsInstrumentationKey string
param appInsightsConnectionString string

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys(resourceId(resourceGroup().name, 'Microsoft.Storage/storageAccounts', storageAccountName), '2019-04-01').keys[0].value};EndpointSuffix=core.windows.net'

resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: resourceNames.functionApp
  location: conventions.location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  tags: conventions.baseTags
  properties: {
    siteConfig: {
      vnetRouteAllEnabled: false
      alwaysOn: false // Always On is not required for Consumption plan and it will incur additional cost if enabled
      netFrameworkVersion: 'v8.0'
      use32BitWorkerProcess: false
      minTlsVersion: '1.2'
      http20Enabled: true
      ftpsState: 'Disabled'
    }
    httpsOnly: true
    serverFarmId: appServicePlanId
  }
}

resource functionAppAppsettings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'appsettings'
  parent: functionApp
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY                : appInsightsInstrumentationKey
    APPLICATIONINSIGHTS_CONNECTION_STRING         : appInsightsConnectionString
    FUNCTIONS_EXTENSION_VERSION                   : '~4'
    FUNCTIONS_WORKER_RUNTIME                      : 'dotnet-isolated'
    AzureWebJobsStorage                           : storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING      : storageAccountConnectionString
    WEBSITE_CONTENTSHARE                          : toLower('functionapp${substring(uniqueString(functionApp.name, resourceNames.resourceGroup),0,6)}')
    WEBSITE_LOAD_USER_PROFILE                     : '1'
    AadTenantId                                   : aadTenantId
    IsvClientId                                   : isvClientId
    EnvironmentName                               : conventions.environment
    AttachmentsStorageBaseURL                     : 'https://${storageAccountName}.blob.${environment().suffixes.storage}'
    AttachmentsContainerName                      : attachmentsContainerName
  }
}

module functionAppStorageDeployment 'functionAppStorage.bicep' = {
  name: take('sa-fnApp-${conventions.deploymentId}', 64)
  params: {
    storageAccountName: storageAccountName
    functionAppName: functionApp.name
    functionIdentityId: functionApp.identity.principalId
  }
}

resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'diagnosticSettings'
  scope: functionApp
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    metrics: [
      {
        enabled: true
        category: 'AllMetrics'
      }
    ]
    logs: [
      {
        enabled: true
        category: 'FunctionAppLogs'
      }
    ]
  }
}

output functionAppIdentity string = functionApp.identity.principalId
output name string = functionApp.name
