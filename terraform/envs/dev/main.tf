locals {
  project     = "finops"
  environment = "dev"
  location    = "eastus"
  owner       = "jordann6"

  common_tags = {
    project     = local.project
    environment = local.environment
    owner       = local.owner
    managed_by  = "terraform"
  }
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.project}-${local.environment}"
  location = local.location
  tags     = local.common_tags
}

module "sample_workload" {
  source = "../../modules/sample_workload"

  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  project             = local.project
  environment         = local.environment
  common_tags         = local.common_tags
}

module "cosmos" {
  source = "../../modules/cosmos"

  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  project             = local.project
  environment         = local.environment
  common_tags         = local.common_tags
}

module "functions" {
  source = "../../modules/functions"

  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  project                    = local.project
  environment                = local.environment
  common_tags                = local.common_tags
  cosmos_account_endpoint    = module.cosmos.account_endpoint
  cosmos_account_name        = module.cosmos.account_name
  cosmos_account_id          = module.cosmos.account_id
  cosmos_database_name       = module.cosmos.database_name
  log_analytics_workspace_id = module.sample_workload.log_analytics_workspace_id
  subscription_id            = var.subscription_id
}

# --- Budget Alerts -----------------------------------------------------------

resource "azurerm_monitor_action_group" "budget_alerts" {
  name                = "ag-${local.project}-budget-${local.environment}"
  resource_group_name = azurerm_resource_group.this.name
  short_name          = "finops-budget"
  tags                = local.common_tags

  email_receiver {
    name                    = "owner"
    email_address           = var.alert_email
    use_common_alert_schema = true
  }
}

resource "azurerm_consumption_budget_subscription" "monthly" {
  name            = "budget-${local.project}-${local.environment}-monthly"
  subscription_id = "/subscriptions/${var.subscription_id}"

  amount     = var.monthly_budget_usd
  time_grain = "Monthly"

  time_period {
    start_date = formatdate("YYYY-MM-01'T'00:00:00'Z'", timestamp())
  }

  notification {
    enabled        = true
    threshold      = 80
    operator       = "GreaterThan"
    threshold_type = "Forecasted"
    contact_groups = [azurerm_monitor_action_group.budget_alerts.id]
  }

  notification {
    enabled        = true
    threshold      = 100
    operator       = "GreaterThanOrEqualTo"
    threshold_type = "Actual"
    contact_groups = [azurerm_monitor_action_group.budget_alerts.id]
  }
}
