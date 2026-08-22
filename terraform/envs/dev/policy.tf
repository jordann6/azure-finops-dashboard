# =============================================================================
# Governance: Azure Policy tag enforcement.
#
# The TagHygieneService (Function) *reports* untagged resources after the fact.
# Azure Policy *governs* them at the control plane. This custom definition uses
# the Audit effect (not Deny/Modify) on purpose: Deny would block the sample
# workload's intentionally-untagged storage account at apply time, and Modify
# would auto-tag it and erase the hygiene demo. Audit flags non-compliant
# resources in the Policy compliance view while leaving the demo intact.
#
# Note: policy compliance is evaluated asynchronously by Azure and can take up
# to ~30 minutes to populate after assignment.
# =============================================================================

resource "azurerm_policy_definition" "require_cost_tag" {
  name         = "audit-require-${local.project}-cost-tag"
  policy_type  = "Custom"
  mode         = "Indexed" # tag-bearing resource types only
  display_name = "Audit resources missing the ${var.required_cost_tag} tag"
  description  = "Flags any taggable resource that does not carry the cost allocation tag used for chargeback."

  metadata = jsonencode({
    category = "Tags"
  })

  parameters = jsonencode({
    tagName = {
      type = "String"
      metadata = {
        displayName = "Tag name"
        description = "Name of the required cost allocation tag."
      }
      defaultValue = var.required_cost_tag
    }
  })

  policy_rule = jsonencode({
    if = {
      field  = "[concat('tags[', parameters('tagName'), ']')]"
      exists = "false"
    }
    then = {
      effect = "audit"
    }
  })
}

resource "azurerm_resource_group_policy_assignment" "require_cost_tag" {
  name                 = "audit-cost-tag"
  resource_group_id    = azurerm_resource_group.this.id
  policy_definition_id = azurerm_policy_definition.require_cost_tag.id
  display_name         = "Audit missing ${var.required_cost_tag} tag"

  parameters = jsonencode({
    tagName = { value = var.required_cost_tag }
  })
}
