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
