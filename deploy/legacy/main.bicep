targetScope = 'resourceGroup'

@allowed(['development', 'staging', 'production'])
param environment string
param location string = resourceGroup().location
param prefix string = 'qbs'
param imageTag string
param deployApplications bool = false
param publicOrigin string
param sqlAdministratorObjectId string
param sqlAdministratorName string
param emailEndpoint string = ''
param emailSender string = ''
param mapsClientId string = ''
param aiEndpoint string = ''
param aiDeployment string = ''
param aiModelVersion string = ''
param administratorEmail string = ''

var suffix = uniqueString(resourceGroup().id, environment)
var name = '${prefix}-${environment}'

resource apiIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = { name: '${name}-api', location: location }
resource workerIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = { name: '${name}-worker', location: location }
resource gatewayIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = { name: '${name}-gateway', location: location }
resource gatewayRegistryRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, gatewayIdentity.id, 'pull')
  scope: registry
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d'), principalId: gatewayIdentity.properties.principalId, principalType: 'ServicePrincipal' }
}
resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: '${prefix}${suffix}'
  location: location
  sku: { name: 'Basic' }
  properties: { adminUserEnabled: false }
}
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: '${prefix}${suffix}'
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: { supportsHttpsTrafficOnly: true, minimumTlsVersion: 'TLS1_2', allowBlobPublicAccess: false, allowSharedKeyAccess: false }
}
resource blobs 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
  properties: {
    cors: { corsRules: [{ allowedOrigins: [publicOrigin], allowedMethods: ['PUT', 'GET', 'HEAD', 'OPTIONS'], allowedHeaders: ['*'], exposedHeaders: ['ETag', 'x-ms-request-id'], maxAgeInSeconds: 300 }] }
  }
}
resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for container in ['photos', 'keys']: {
  parent: blobs
  name: container
  properties: { publicAccess: 'None' }
}]
resource queues 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = { parent: storage, name: 'default', properties: {} }
resource processingQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = { parent: queues, name: 'processing', properties: {} }
resource vault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${prefix}-${suffix}'
  location: location
  properties: { tenantId: tenant().tenantId, sku: { family: 'A', name: 'standard' }, enableRbacAuthorization: true, enableSoftDelete: true, enablePurgeProtection: true, softDeleteRetentionInDays: 90 }
}
resource protectionKey 'Microsoft.KeyVault/vaults/keys@2023-07-01' = { parent: vault, name: 'data-protection', properties: { kty: 'RSA', keySize: 2048, keyOps: ['wrapKey', 'unwrapKey'] } }
resource sql 'Microsoft.Sql/servers@2023-08-01' = {
  name: '${name}-${suffix}'
  location: location
  properties: { version: '12.0', minimalTlsVersion: '1.2', administrators: { administratorType: 'ActiveDirectory', principalType: 'Group', login: sqlAdministratorName, sid: sqlAdministratorObjectId, tenantId: tenant().tenantId, azureADOnlyAuthentication: true } }
}
resource database 'Microsoft.Sql/servers/databases@2023-08-01' = { parent: sql, name: 'studio', location: location, sku: { name: 'Basic', tier: 'Basic', capacity: 5 } }
resource sqlFirewall 'Microsoft.Sql/servers/firewallRules@2023-08-01' = { parent: sql, name: 'AzureServices', properties: { startIpAddress: '0.0.0.0', endIpAddress: '0.0.0.0' } }
resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = { name: '${name}-logs', location: location, properties: { retentionInDays: 30, sku: { name: 'PerGB2018' } } }
resource insights 'Microsoft.Insights/components@2020-02-02' = { name: '${name}-insights', location: location, kind: 'web', properties: { Application_Type: 'web', WorkspaceResourceId: logs.id } }
resource host 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${name}-hosts'
  location: location
  properties: { appLogsConfiguration: { destination: 'log-analytics', logAnalyticsConfiguration: { customerId: logs.properties.customerId, sharedKey: logs.listKeys().primarySharedKey } } }
}
var identities = [apiIdentity.id, workerIdentity.id]
resource blobRoles 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for identity in identities: {
  name: guid(storage.id, identity, 'blob')
  scope: storage
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'), principalId: identity == apiIdentity.id ? apiIdentity.properties.principalId : workerIdentity.properties.principalId, principalType: 'ServicePrincipal' }
}]
resource queueRoles 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for identity in identities: {
  name: guid(storage.id, identity, 'queue')
  scope: storage
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88'), principalId: identity == apiIdentity.id ? apiIdentity.properties.principalId : workerIdentity.properties.principalId, principalType: 'ServicePrincipal' }
}]
resource keyRoles 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for identity in identities: {
  name: guid(vault.id, identity, 'crypto')
  scope: vault
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '12338af0-0e69-4776-bea7-57ae8d297424'), principalId: identity == apiIdentity.id ? apiIdentity.properties.principalId : workerIdentity.properties.principalId, principalType: 'ServicePrincipal' }
}]
resource registryRoles 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for identity in identities: {
  name: guid(registry.id, identity, 'pull')
  scope: registry
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d'), principalId: identity == apiIdentity.id ? apiIdentity.properties.principalId : workerIdentity.properties.principalId, principalType: 'ServicePrincipal' }
}]
var sharedEnvironment = [
  { name: 'Gateway__TrustForwardedHeaders', value: 'true' }
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'DOTNET_ENVIRONMENT', value: 'Production' }
  { name: 'PublicOrigin', value: publicOrigin }
  { name: 'Azure__BlobEndpoint', value: storage.properties.primaryEndpoints.blob }
  { name: 'Azure__QueueEndpoint', value: '${storage.properties.primaryEndpoints.queue}processing' }
  { name: 'Azure__EmailEndpoint', value: emailEndpoint }
  { name: 'Azure__EmailSender', value: emailSender }
  { name: 'Azure__MapsClientId', value: mapsClientId }
  { name: 'Azure__AiEndpoint', value: aiEndpoint }
  { name: 'Azure__AiDeployment', value: aiDeployment }
  { name: 'Azure__AiModelVersion', value: aiModelVersion }
  { name: 'Retention__AdministratorEmail', value: administratorEmail }
  { name: 'DataProtection__BlobUri', value: '${storage.properties.primaryEndpoints.blob}keys/keyring.xml' }
  { name: 'DataProtection__KeyUri', value: '${vault.properties.vaultUri}keys/data-protection' }
  { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: insights.properties.ConnectionString }
]
resource api 'Microsoft.App/containerApps@2024-03-01' = if (deployApplications) {
  name: '${name}-api'
  location: location
  identity: { type: 'UserAssigned', userAssignedIdentities: { '${apiIdentity.id}': {} } }
  properties: {
    managedEnvironmentId: host.id
    configuration: { ingress: { external: false, targetPort: 8080, transport: 'http', allowInsecure: false }, registries: [{ server: registry.properties.loginServer, identity: apiIdentity.id }] }
    template: { containers: [{ name: 'api', image: '${registry.properties.loginServer}/qbs-api:${imageTag}', resources: { cpu: json('0.5'), memory: '1Gi' }, env: concat(sharedEnvironment, [{ name: 'AZURE_CLIENT_ID', value: apiIdentity.properties.clientId }, { name: 'ConnectionStrings__Studio', value: 'Server=tcp:${sql.properties.fullyQualifiedDomainName},1433;Database=studio;Authentication=Active Directory Managed Identity;User Id=${apiIdentity.properties.clientId};Encrypt=True;' }]) }], scale: { minReplicas: 1, maxReplicas: 3 } }
  }
  dependsOn: [blobRoles, queueRoles, keyRoles, registryRoles]
}
resource worker 'Microsoft.App/containerApps@2024-03-01' = if (deployApplications) {
  name: '${name}-worker'
  location: location
  identity: { type: 'UserAssigned', userAssignedIdentities: { '${workerIdentity.id}': {} } }
  properties: {
    managedEnvironmentId: host.id
    configuration: { registries: [{ server: registry.properties.loginServer, identity: workerIdentity.id }] }
    template: { containers: [{ name: 'worker', image: '${registry.properties.loginServer}/qbs-worker:${imageTag}', resources: { cpu: 1, memory: '2Gi' }, env: concat(sharedEnvironment, [{ name: 'AZURE_CLIENT_ID', value: workerIdentity.properties.clientId }, { name: 'ConnectionStrings__Studio', value: 'Server=tcp:${sql.properties.fullyQualifiedDomainName},1433;Database=studio;Authentication=Active Directory Managed Identity;User Id=${workerIdentity.properties.clientId};Encrypt=True;' }]) }], scale: { minReplicas: 1, maxReplicas: 1 } }
  }
  dependsOn: [blobRoles, queueRoles, keyRoles, registryRoles]
}
resource gateway 'Microsoft.App/containerApps@2024-03-01' = if (deployApplications) {
  name: '${name}-gateway'
  location: location
  identity: { type: 'UserAssigned', userAssignedIdentities: { '${gatewayIdentity.id}': {} } }
  properties: {
    managedEnvironmentId: host.id
    configuration: { ingress: { external: true, targetPort: 8080, transport: 'http', allowInsecure: false }, registries: [{ server: registry.properties.loginServer, identity: gatewayIdentity.id }] }
    template: { containers: [{ name: 'gateway', image: '${registry.properties.loginServer}/qbs-gateway:${imageTag}', resources: { cpu: json('0.5'), memory: '1Gi' }, env: [{ name: 'API_UPSTREAM', value: 'https://${api!.properties.configuration.ingress.fqdn}' }] }], scale: { minReplicas: 1, maxReplicas: 3 } }
  }
  dependsOn: [gatewayRegistryRole]
}
resource catalog 'Microsoft.Web/staticSites@2023-12-01' = { name: '${name}-design-system', location: location, sku: { name: 'Free', tier: 'Free' }, properties: {} }

output registryServer string = registry.properties.loginServer
output sqlServer string = sql.properties.fullyQualifiedDomainName
output apiIdentityName string = apiIdentity.name
output workerIdentityName string = workerIdentity.name
output catalogHost string = catalog.properties.defaultHostname
output gatewayHost string = deployApplications ? gateway!.properties.configuration.ingress.fqdn : ''
