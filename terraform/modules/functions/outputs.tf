output "function_app_name" {
  value = azurerm_linux_function_app.this.name
}

output "function_app_hostname" {
  value = azurerm_linux_function_app.this.default_hostname
}

output "function_app_principal_id" {
  value = azurerm_linux_function_app.this.identity[0].principal_id
}

output "application_insights_name" {
  value = azurerm_application_insights.this.name
}
