output "resource_group_name" {
  value = azurerm_resource_group.this.name
}

output "location" {
  value = azurerm_resource_group.this.location
}

output "app_storage_account_id" {
  value = module.sample_workload.app_storage_account_id
}

output "untagged_storage_account_id" {
  value = module.sample_workload.untagged_storage_account_id
}

output "log_analytics_workspace_id" {
  value = module.sample_workload.log_analytics_workspace_id
}

output "cosmos_account_endpoint" {
  value = module.cosmos.account_endpoint
}

output "cosmos_database_name" {
  value = module.cosmos.database_name
}

output "function_app_name" {
  value = module.functions.function_app_name
}

output "function_app_hostname" {
  value = module.functions.function_app_hostname
}
