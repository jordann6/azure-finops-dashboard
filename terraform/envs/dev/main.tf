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
  cosmos_database_name       = module.cosmos.database_name
  log_analytics_workspace_id = module.sample_workload.log_analytics_workspace_id
  subscription_id            = "9c644a73-5dc1-4bfe-9e90-91865014cdd2"
}
