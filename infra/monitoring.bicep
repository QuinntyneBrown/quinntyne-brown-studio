param location string
param vmName string
param vmId string
param identityId string
param workspaceId string
param insightsId string
param origin string
param administratorEmail string

resource vm 'Microsoft.Compute/virtualMachines@2024-07-01' existing = { name: vmName }
resource agent 'Microsoft.Compute/virtualMachines/extensions@2024-07-01' = {
  parent: vm
  name: 'AzureMonitorLinuxAgent'
  location: location
  properties: {
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorLinuxAgent'
    typeHandlerVersion: '1.0'
    autoUpgradeMinorVersion: true
    enableAutomaticUpgrade: true
    settings: { authentication: { managedIdentity: { 'identifier-name': 'mi_res_id', 'identifier-value': identityId } } }
  }
}
resource rule 'Microsoft.Insights/dataCollectionRules@2023-03-11' = {
  name: 'qbs-host'
  location: location
  kind: 'Linux'
  properties: {
    dataSources: { syslog: [{ name: 'system', streams: ['Microsoft-Syslog'], facilityNames: ['daemon', 'syslog'], logLevels: ['Warning', 'Error', 'Critical', 'Alert', 'Emergency'] }] }
    destinations: { logAnalytics: [{ name: 'logs', workspaceResourceId: workspaceId }] }
    dataFlows: [{ streams: ['Microsoft-Syslog'], destinations: ['logs'] }]
  }
}
resource association 'Microsoft.Insights/dataCollectionRuleAssociations@2023-03-11' = {
  name: 'qbs-monitor'
  scope: vm
  properties: { dataCollectionRuleId: rule.id }
}
resource actions 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'qbs-operations'
  location: 'global'
  properties: { groupShortName: 'qbs', enabled: true, emailReceivers: [{ name: 'studio', emailAddress: administratorEmail, useCommonAlertSchema: true }] }
}
resource heartbeat 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: 'qbs-heartbeat'
  location: location
  kind: 'LogAlert'
  properties: {
    displayName: 'Studio VM heartbeat missing'
    enabled: true
    severity: 1
    skipQueryValidation: true
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    scopes: [workspaceId]
    criteria: { allOf: [{ query: 'Heartbeat | where _ResourceId =~ "${vmId}"', timeAggregation: 'Count', operator: 'LessThan', threshold: 1, failingPeriods: { numberOfEvaluationPeriods: 1, minFailingPeriodsToAlert: 1 } }] }
    actions: { actionGroups: [actions.id] }
  }
}
resource availability 'Microsoft.Insights/webtests@2022-06-15' = {
  name: 'qbs-https'
  location: location
  kind: 'standard'
  tags: { 'hidden-link:${insightsId}': 'Resource' }
  properties: {
    SyntheticMonitorId: 'qbs-https'
    Name: 'Studio HTTPS'
    Enabled: true
    Frequency: 300
    Timeout: 30
    Kind: 'standard'
    RetryEnabled: true
    Locations: [{ Id: 'us-va-ash-azr' }, { Id: 'us-il-ch1-azr' }, { Id: 'us-ca-sjc-azr' }]
    Request: { RequestUrl: '${origin}/api/health', HttpVerb: 'GET' }
    ValidationRules: { ExpectedHttpStatusCode: 200, SSLCheck: true, SSLCertRemainingLifetimeCheck: 7 }
  }
}
resource availabilityAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'qbs-https-unavailable'
  location: 'global'
  properties: {
    severity: 1
    enabled: true
    scopes: [availability.id, insightsId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: { 'odata.type': 'Microsoft.Azure.Monitor.WebtestLocationAvailabilityCriteria', webTestId: availability.id, componentId: insightsId, failedLocationCount: 2 }
    actions: [{ actionGroupId: actions.id }]
  }
}
