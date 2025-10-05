
# Variables
variable "AWS_REGION" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "AWS_ACCESS_KEY_ID" {
  description = "Access key to AWS console"
  type        = string
  sensitive   = true
}

variable "AWS_SECRET_ACCESS_KEY" {
  description = "Secret key to AWS console"
  type        = string
  sensitive   = true
}


variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}


