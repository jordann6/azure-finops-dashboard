resource "azurerm_storage_account" "functions" {
  name                            = "stfinopsfunc${var.environment}"
  resource_group_name             = var.resource_group_name
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  https_traffic_only_enabled      = true
  allow_nested_items_to_be_public = false

  tags = merge(var.common_tags, {
    purpose = "functions_storage"
  })
}

resource "azurerm_application_insights" "this" {
  name                = "appi-${var.project}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  workspace_id        = var.log_analytics_workspace_id
  application_type    = "web"

  tags = var.common_tags
}

resource "azurerm_service_plan" "this" {
  name                = "asp-${var.project}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = "Y1"

  tags = var.common_tags
}

resource "azurerm_linux_function_app" "this" {
  name                       = "func-${var.project}-${var.environment}-jn6"
  resource_group_name        = var.resource_group_name
  location                   = var.location
  service_plan_id            = azurerm_service_plan.this.id
  storage_account_name       = azurerm_storage_account.functions.name
  storage_account_access_key = azurerm_storage_account.functions.primary_access_key

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }

    application_insights_connection_string = azurerm_application_insights.this.connection_string
  }

  app_settings = {
    "CosmosDb__Endpoint"     = var.cosmos_account_endpoint
    "CosmosDb__DatabaseName" = var.cosmos_database_name
    "Azure__SubscriptionId"  = var.subscription_id
    # Set to the Static Web Apps URL after deployment to restrict CORS.
    # Example: "https://your-app.azurestaticapps.net"
    "CORS_ALLOWED_ORIGIN"    = "*"
  }

  tags = var.common_tags
}

# Grant the Function App read access to cost data
resource "azurerm_role_assignment" "cost_reader" {
  scope                = "/subscriptions/${var.subscription_id}"
  role_definition_name = "Cost Management Reader"
  principal_id         = azurerm_linux_function_app.this.identity[0].principal_id
}

# Grant the Function App read access to resource metadata (for tag hygiene)
resource "azurerm_role_assignment" "reader" {
  scope                = "/subscriptions/${var.subscription_id}"
  role_definition_name = "Reader"
  principal_id         = azurerm_linux_function_app.this.identity[0].principal_id
}

# Grant the Function App data plane access to Cosmos DB (database scope)
resource "azurerm_cosmosdb_sql_role_assignment" "data_contributor" {
  resource_group_name = var.resource_group_name
  account_name        = var.cosmos_account_name
  role_definition_id  = "/subscriptions/${var.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.DocumentDB/databaseAccounts/${var.cosmos_account_name}/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002"
  principal_id        = azurerm_linux_function_app.this.identity[0].principal_id
  scope               = "${var.cosmos_account_id}/dbs/${var.cosmos_database_name}"
}
