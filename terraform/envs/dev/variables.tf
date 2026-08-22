variable "subscription_id" {
  type        = string
  description = "Azure subscription ID. Set via terraform.tfvars (not committed) or TF_VAR_subscription_id env var."
  sensitive   = true
}

variable "alert_email" {
  type        = string
  description = "Email address for budget breach alerts."
}

variable "monthly_budget_usd" {
  type        = number
  description = "Monthly spend limit in USD. Alerts fire at 80% forecast and 100% actual."
  default     = 50
}

variable "cost_center" {
  type        = string
  description = "Cost allocation dimension — who to bill for this spend."
  default     = "platform-eng"
}

variable "team" {
  type        = string
  description = "Owning team, second cost allocation dimension."
  default     = "cloud-platform"
}

variable "required_cost_tag" {
  type        = string
  description = "Tag key the governance policy audits every resource for."
  default     = "cost_center"
}
