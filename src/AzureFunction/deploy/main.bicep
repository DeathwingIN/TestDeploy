@description('Environment short name.')
@allowed([
  'dev'
  'uat'
  'prd'
])
param environment string = 'dev'

param location string = resourceGroup().location
@description('Skip Token Validation')
param skipTokenValidation string = 'false'

@description('Business Central Webhook Endpoint')
param bcWebhookEndpoint string = ''

@description('Business Central Environment Name')
param bcEnvironmentName string = 'Production'

@description('Business Central Company ID')
param bcCompanyId string = ''

@description('Business Central Company Name')
param bcCompanyName string = ''

@description('Azure Client ID for accessing Key Vault')
param azureClientId string = ''

@secure()
param azureClientSecret string = ''

@description('Release Id for deployment name')
param releaseId string = 'manual-local-${utcNow('ddMMyyyyHHmm')}-1'

@description('AAD Tenant ID')
param aadTenantId string = ''

@description('ISV Client ID')
param isvClientId string = ''

// Variables
var licensingCode = 'licensing'
var licensingSuffix = 'mc-licensing-${toLower(environment)}'
var storageAccountName = 'stgmclicensing${environment}'
var appUniqueSuffix = substring(uniqueString(storageAccountName), 0, 4)
var releaseIdParts = split(releaseId, '-')
var deploymentId = '${releaseIdParts[2]}-${releaseIdParts[3]}'
var attachmentsContainerName = 'attachments'

var conventions = {
  location: location
  environment: environment
  deploymentId: deploymentId
  baseTags: {
    releaseId: toLower(releaseId)
    environment: environment
    component: licensingCode
    use: 'integration'
  }
}

var resourceNames = {
  resourceGroup: resourceGroup().name
  functionApp: take('fap-${licensingSuffix}', 127)
  storageAccount: {
    name: take('${storageAccountName}${appUniqueSuffix}', 24)
    //checkpointTableName: 'keepalivecheckpoints'
  }
  appServicePlan: 'asp-${licensingSuffix}'
  appInsights: 'ai-${licensingSuffix}'
  logAnalytics: 'law-${licensingSuffix}'
}

// resources

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: resourceNames.logAnalytics
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceNames.appInsights
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// modules
module storageAccoutDeployment 'modules/storageAccount.bicep' = {
  name: take('sa-${deploymentId}', 64)
  params: {
    conventions: conventions
    resourceNames: resourceNames
    attachmentsContainerName: attachmentsContainerName
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
  }
}

module appServicePlanDeployment 'modules/appServicePlan.bicep' = {
  name: take('asp-${deploymentId}', 64)
  params: {
    conventions: conventions
    resourceNames: resourceNames
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
  }
}

module functionAppDeployment 'modules/functionApp.bicep' = {
  name: take('fa-${deploymentId}', 64)
  params: {
    conventions: conventions
    resourceNames: resourceNames
    appServicePlanId: appServicePlanDeployment.outputs.id
    storageAccountName: storageAccoutDeployment.outputs.name
    attachmentsContainerName: attachmentsContainerName
    aadTenantId: aadTenantId
    isvClientId: isvClientId
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
    appInsightsInstrumentationKey: appInsights.properties.InstrumentationKey
    appInsightsConnectionString: appInsights.properties.ConnectionString

    skipTokenValidation: skipTokenValidation
    bcWebhookEndpoint: bcWebhookEndpoint
    bcEnvironmentName: bcEnvironmentName
    bcCompanyId: bcCompanyId
    bcCompanyName: bcCompanyName
    azureClientId: azureClientId
    azureClientSecret: azureClientSecret
  }
}

output functionAppName string = functionAppDeployment.outputs.name
