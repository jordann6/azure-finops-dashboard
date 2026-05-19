variable "subscription_id" {
  type        = string
  description = "Azure subscription ID. Set via terraform.tfvars (not committed) or TF_VAR_subscription_id env var."
  sensitive   = true
}
