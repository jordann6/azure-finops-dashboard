variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "project" {
  type = string
}

variable "environment" {
  type = string
}

variable "common_tags" {
  type = map(string)
}

variable "cosmos_account_endpoint" {
  type = string
}

variable "cosmos_account_name" {
  type = string
}

variable "cosmos_database_name" {
  type = string
}

variable "log_analytics_workspace_id" {
  type = string
}

variable "subscription_id" {
  type = string
}
