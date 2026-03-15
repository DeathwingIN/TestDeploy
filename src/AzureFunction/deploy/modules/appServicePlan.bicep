param conventions object
param resourceNames object
param logAnalyticsWorkspaceId string

var environmentType = conventions.environment == 'prd' ? 'prod' : 'nonprod'

@description('Configurations based on the environment type.')
var environmentConfigurationMap = {
  nonprod: {
    sku: {
      name: 'Y1' //B1 plan for dedicated server which is unecessary for the moment 
      tier: 'Dynamic'
      //family: 'B' 
      //capacity: 1
    }
    zoneRedundant: false
  }
  prod: {
    sku: {
      name: 'Y1'
      tier: 'Dynamic'
      //family: 'B'
      //capacity: 1
    }
    zoneRedundant: false
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: resourceNames.appServicePlan
  location: conventions.location
  kind: 'functionapp'
  tags: conventions.baseTags
  properties: {
    zoneRedundant: environmentConfigurationMap[environmentType].zoneRedundant
  }
  sku: environmentConfigurationMap[environmentType].sku
}

resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'diagnosticSettingsAppServicePlan'
  scope: appServicePlan
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    metrics: [
      {
        enabled: true
        category: 'AllMetrics'
      }
    ]
  }
}

output id string = appServicePlan.id
