targetScope = 'resourceGroup'

param location string = 'canadacentral'
param vmSize string = 'Standard_B2als_v2'
param sshPublicKey string
param adminUsername string = 'studioadmin'
param deployPrincipalId string
param sqlAdministratorObjectId string
param sqlAdministratorName string
param administratorEmail string
param aiLocation string = 'canadaeast'
param aiModel string = 'gpt-4.1-mini'
param aiModelVersion string = '2025-04-14'
param aiCapacity int = 1
@description('Leave empty for the temporary Azure hostname. Set only after custom DNS is ready.')
param publicOrigin string = ''

var suffix = uniqueString(resourceGroup().id)
var hostname = 'qbs-${suffix}.${location}.cloudapp.azure.com'
var origin = empty(publicOrigin) ? 'https://${hostname}' : publicOrigin

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-qbs-prod'
  location: location
}
resource publicIp 'Microsoft.Network/publicIPAddresses@2024-05-01' = {
  name: 'pip-qbs'
  location: location
  sku: { name: 'Standard' }
  properties: {
    publicIPAllocationMethod: 'Static'
    dnsSettings: { domainNameLabel: 'qbs-${suffix}' }
  }
}
resource nsg 'Microsoft.Network/networkSecurityGroups@2024-05-01' = {
  name: 'nsg-qbs'
  location: location
  properties: {
    securityRules: [for port in [80, 443]: {
      name: 'public-${port}'
      properties: {
        priority: 100 + port
        direction: 'Inbound'
        access: 'Allow'
        protocol: 'Tcp'
        sourcePortRange: '*'
        destinationPortRange: string(port)
        sourceAddressPrefix: 'Internet'
        destinationAddressPrefix: '*'
      }
    }]
  }
}
resource network 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: 'vnet-qbs'
  location: location
  properties: {
    addressSpace: { addressPrefixes: ['10.70.0.0/16'] }
    subnets: [{ name: 'studio', properties: { addressPrefix: '10.70.1.0/24' } }]
  }
}
resource nic 'Microsoft.Network/networkInterfaces@2024-05-01' = {
  name: 'nic-qbs'
  location: location
  properties: {
    networkSecurityGroup: { id: nsg.id }
    ipConfigurations: [{
      name: 'primary'
      properties: {
        privateIPAllocationMethod: 'Dynamic'
        subnet: { id: network.properties.subnets[0].id }
        publicIPAddress: { id: publicIp.id }
      }
    }]
  }
}
resource vm 'Microsoft.Compute/virtualMachines@2024-07-01' = {
  name: 'vm-qbs'
  location: location
  identity: { type: 'UserAssigned', userAssignedIdentities: { '${identity.id}': {} } }
  properties: {
    hardwareProfile: { vmSize: vmSize }
    securityProfile: { securityType: 'TrustedLaunch', uefiSettings: { secureBootEnabled: true, vTpmEnabled: true } }
    storageProfile: {
      imageReference: { publisher: 'Canonical', offer: 'ubuntu-24_04-lts', sku: 'server', version: 'latest' }
      osDisk: { createOption: 'FromImage', diskSizeGB: 32, managedDisk: { storageAccountType: 'StandardSSD_LRS' } }
    }
    osProfile: {
      computerName: 'qbs'
      adminUsername: adminUsername
      linuxConfiguration: {
        disablePasswordAuthentication: true
        provisionVMAgent: true
        ssh: { publicKeys: [{ path: '/home/${adminUsername}/.ssh/authorized_keys', keyData: sshPublicKey }] }
        patchSettings: { patchMode: 'AutomaticByPlatform', assessmentMode: 'AutomaticByPlatform' }
      }
    }
    networkProfile: { networkInterfaces: [{ id: nic.id }] }
  }
}
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'qbs${suffix}'
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: { supportsHttpsTrafficOnly: true, minimumTlsVersion: 'TLS1_2', allowBlobPublicAccess: false, allowSharedKeyAccess: false }
}
resource blobs 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
  properties: {
    deleteRetentionPolicy: { enabled: true, days: 30 }
    containerDeleteRetentionPolicy: { enabled: true, days: 30 }
    isVersioningEnabled: true
    cors: { corsRules: [{ allowedOrigins: [origin], allowedMethods: ['PUT', 'GET', 'HEAD', 'OPTIONS'], allowedHeaders: ['*'], exposedHeaders: ['ETag', 'x-ms-request-id'], maxAgeInSeconds: 300 }] }
  }
}
resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for name in ['photos', 'keys', 'releases']: {
  parent: blobs
  name: name
  properties: { publicAccess: 'None' }
}]
resource queues 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = { parent: storage, name: 'default', properties: {} }
resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = { parent: queues, name: 'processing', properties: {} }
resource vault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'qbs-${suffix}'
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: { family: 'A', name: 'standard' }
    enableRbacAuthorization: true
    enableSoftDelete: true
    enablePurgeProtection: true
    softDeleteRetentionInDays: 90
  }
}
resource key 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  parent: vault
  name: 'data-protection'
  properties: { kty: 'RSA', keySize: 2048, keyOps: ['wrapKey', 'unwrapKey'] }
}
resource sql 'Microsoft.Sql/servers@2023-08-01' = {
  name: 'qbs-${suffix}'
  location: location
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Application'
      login: sqlAdministratorName
      sid: sqlAdministratorObjectId
      tenantId: tenant().tenantId
      azureADOnlyAuthentication: true
    }
  }
}
resource database 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: sql
  name: 'studio'
  location: location
  sku: { name: 'Basic', tier: 'Basic', capacity: 5 }
  properties: { maxSizeBytes: 2147483648 }
}
resource retention 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2023-08-01' = {
  parent: database
  name: 'default'
  properties: { retentionDays: 7 }
}
resource sqlFirewall 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sql
  name: 'StudioVM'
  properties: { startIpAddress: publicIp.properties.ipAddress, endIpAddress: publicIp.properties.ipAddress }
}
resource email 'Microsoft.Communication/emailServices@2023-03-31' = {
  name: 'qbs-email-${suffix}'
  location: 'global'
  properties: { dataLocation: 'Canada' }
}
resource emailDomain 'Microsoft.Communication/emailServices/domains@2023-03-31' = {
  parent: email
  name: 'AzureManagedDomain'
  location: 'global'
  properties: { domainManagement: 'AzureManaged' }
}
resource communication 'Microsoft.Communication/communicationServices@2023-03-31' = {
  name: 'qbs-communication-${suffix}'
  location: 'global'
  properties: { dataLocation: 'Canada', linkedDomains: [emailDomain.id] }
}
resource maps 'Microsoft.Maps/accounts@2023-06-01' = {
  name: 'qbs-maps-${suffix}'
  location: 'global'
  kind: 'Gen2'
  sku: { name: 'G2' }
  properties: { disableLocalAuth: true }
}
resource ai 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: 'qbs-ai-${suffix}'
  location: aiLocation
  kind: 'OpenAI'
  sku: { name: 'S0' }
  properties: { customSubDomainName: 'qbs-ai-${suffix}', disableLocalAuth: true, publicNetworkAccess: 'Enabled' }
}
resource model 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: ai
  name: 'photo-vision'
  sku: { name: 'Standard', capacity: aiCapacity }
  properties: { model: { format: 'OpenAI', name: aiModel, version: aiModelVersion }, versionUpgradeOption: 'NoAutoUpgrade' }
}
resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'qbs-logs'
  location: location
  properties: { retentionInDays: 30, sku: { name: 'PerGB2018' } }
}
resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'qbs-insights'
  location: location
  kind: 'web'
  properties: { Application_Type: 'web', WorkspaceResourceId: logs.id }
}

resource blobRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, identity.id, 'blob')
  scope: storage
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'), principalId: identity.properties.principalId, principalType: 'ServicePrincipal' }
}
resource queueRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, identity.id, 'queue')
  scope: storage
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88'), principalId: identity.properties.principalId, principalType: 'ServicePrincipal' }
}
resource keyRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vault.id, identity.id, 'crypto')
  scope: vault
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '12338af0-0e69-4776-bea7-57ae8d297424'), principalId: identity.properties.principalId, principalType: 'ServicePrincipal' }
}
resource aiRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(ai.id, identity.id, 'ai')
  scope: ai
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'), principalId: identity.properties.principalId, principalType: 'ServicePrincipal' }
}
resource mapsRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(maps.id, identity.id, 'maps')
  scope: maps
  // Azure Maps Data Reader. The calculator reads both search and route services.
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '423170ca-a8f6-4b0f-8487-9e4eb8f49bfa'), principalId: identity.properties.principalId, principalType: 'ServicePrincipal' }
}
resource emailRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(communication.id, identity.id, 'email')
  scope: communication
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c'), principalId: identity.properties.principalId, principalType: 'ServicePrincipal' }
}
resource releaseRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containers[2].id, deployPrincipalId, 'release')
  scope: containers[2]
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'), principalId: deployPrincipalId, principalType: 'ServicePrincipal' }
}
resource deployReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, deployPrincipalId, 'reader')
  properties: { roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'acdd72a7-3385-48ef-bd42-f606fba81ae7'), principalId: deployPrincipalId, principalType: 'ServicePrincipal' }
}
resource runRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: guid(resourceGroup().id, 'qbs-run-command')
  properties: {
    roleName: 'QBS release command ${suffix}'
    description: 'Execute and inspect managed release commands on the studio VM.'
    type: 'CustomRole'
    assignableScopes: [resourceGroup().id]
    permissions: [{ actions: ['Microsoft.Compute/virtualMachines/read', 'Microsoft.Compute/virtualMachines/runCommands/read', 'Microsoft.Compute/virtualMachines/runCommands/write', 'Microsoft.Compute/virtualMachines/runCommands/delete'], notActions: [], dataActions: [], notDataActions: [] }]
  }
}
resource runAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vm.id, deployPrincipalId, 'run')
  scope: vm
  properties: { roleDefinitionId: runRole.id, principalId: deployPrincipalId, principalType: 'ServicePrincipal' }
}

module monitoring './monitoring.bicep' = {
  name: 'monitoring'
  params: { location: location, vmName: vm.name, vmId: vm.id, identityId: identity.id, workspaceId: logs.id, insightsId: insights.id, origin: origin, administratorEmail: administratorEmail }
}

output host object = {
  clientId: identity.properties.clientId
  principalId: identity.properties.principalId
  storage: storage.name
  origin: origin
  vmId: vm.id
  location: location
  sqlServer: sql.name
  environment: {
    ASPNETCORE_ENVIRONMENT: 'Production'
    DOTNET_ENVIRONMENT: 'Production'
    ASPNETCORE_URLS: 'http://127.0.0.1:7444'
    PublicOrigin: origin
    AZURE_CLIENT_ID: identity.properties.clientId
    ConnectionStrings__Studio: 'Server=tcp:${sql.properties.fullyQualifiedDomainName},1433;Database=studio;Authentication=Active Directory Managed Identity;User Id=${identity.properties.clientId};Encrypt=True;TrustServerCertificate=False'
    Azure__BlobEndpoint: storage.properties.primaryEndpoints.blob
    Azure__QueueEndpoint: '${storage.properties.primaryEndpoints.queue}processing'
    Azure__EmailEndpoint: 'https://${communication.properties.hostName}'
    Azure__EmailSender: 'DoNotReply@${emailDomain.properties.fromSenderDomain}'
    Azure__MapsClientId: maps.properties.uniqueId
    Azure__AiEndpoint: ai.properties.endpoint
    Azure__AiDeployment: model.name
    Azure__AiModelVersion: aiModelVersion
    Retention__AdministratorEmail: administratorEmail
    DataProtection__BlobUri: '${storage.properties.primaryEndpoints.blob}keys/keyring.xml'
    DataProtection__KeyUri: '${vault.properties.vaultUri}keys/data-protection'
    APPLICATIONINSIGHTS_CONNECTION_STRING: insights.properties.ConnectionString
    Raw__Executable: '/usr/bin/dcraw_emu'
  }
}
