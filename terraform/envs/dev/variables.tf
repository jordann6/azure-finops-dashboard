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
