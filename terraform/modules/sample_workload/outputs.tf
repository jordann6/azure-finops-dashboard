output "app_storage_account_id" {
  value = azurerm_storage_account.app_storage.id
}

output "untagged_storage_account_id" {
  value = azurerm_storage_account.untagged.id
}

output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.this.id
}
