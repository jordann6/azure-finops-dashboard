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
