resource "azurerm_storage_account" "app_storage" {
  name                     = "stfinopsapp${var.environment}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  tags = merge(var.common_tags, {
    purpose = "application_storage"
    team    = "engineering"
  })
}

resource "azurerm_storage_account" "untagged" {
  name                     = "stfinopsorphan${var.environment}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  # Intentionally no tags. This resource will appear
  # as a tagging hygiene finding in the dashboard.
}

resource "azurerm_log_analytics_workspace" "this" {
  name                = "law-${var.project}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = merge(var.common_tags, {
    purpose = "observability"
  })
}
