@description('Name of the storage account')
param storageAccountName string

@description('Function app name')
param functionAppName string

@description('Principal Id of a function')
param functionIdentityId string

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' existing = {
  name: storageAccountName
}

@description('This is the built-in Storage Blob Data Contributor role. See https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-blob-data-contributor')
resource storageBlobDataContributor 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

@description('This is the built-in Storage Table Data Contributor role. See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-table-data-contributor')
resource storageTableDataContributor 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}

resource blobServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionIdentityId)
  properties: {
    description: 'Allow managed identity access to blob service as a contributor'
    principalId: functionIdentityId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataContributor.id
  }
}

resource tableServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionIdentityId, 'table')
  properties: {
    description: 'Allow managed identity access to table service as a contributor'
    principalId: functionIdentityId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageTableDataContributor.id
  }
}
