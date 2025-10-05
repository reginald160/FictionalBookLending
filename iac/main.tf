terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }
}

# -------------------------------
# Provider Configuration
# -------------------------------
provider "aws" {
  region     = var.AWS_REGION
  access_key = var.AWS_ACCESS_KEY_ID
  secret_key = var.AWS_SECRET_ACCESS_KEY
}

# -------------------------------
# Data Source: Default VPC
# -------------------------------
data "aws_vpc" "default" {
  default = true
}

# -------------------------------
# Security Group for Redis
# -------------------------------
resource "aws_security_group" "redis_sg" {
  name        = "redis-sg"
  description = "Security group for Redis cluster"
  vpc_id      = data.aws_vpc.default.id

  ingress {
    from_port   = 6389
    to_port     = 6389
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"] # ⚠️ Open to world for testing only
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "redis-security-group"
  }
}

# -------------------------------
# ElastiCache Redis Cluster
# -------------------------------
resource "aws_elasticache_cluster" "fiction_book_redis" {
  cluster_id           = "fiction-book-db"
  engine               = "redis"
  node_type            = "cache.t3.micro"
  num_cache_nodes      = 1
  engine_version       = "7.1"
  port                 = 6389
  parameter_group_name = "default.redis7"
  security_group_ids   = [aws_security_group.redis_sg.id]

  tags = {
    Name        = "fiction_book_redis"
    Environment = var.environment
  }
}

# -------------------------------
# Store Redis Connection String in SSM
# -------------------------------
resource "aws_ssm_parameter" "redis_connection_string" {
  name        = "/${var.environment}/redis/connection-string"
  description = "Redis connection string for Fiction Book Lending"
  type        = "SecureString"
  value       = "${aws_elasticache_cluster.fiction_book_redis.cache_nodes[0].address}:${aws_elasticache_cluster.fiction_book_redis.port}"

  tags = {
    Environment = var.environment
    Application = "FictionBookLending"
  }
}

# -------------------------------
# SQS Queues
# -------------------------------
resource "aws_sqs_queue" "main_queue" {
  name                       = "${var.environment}-book-lending-queue"
  delay_seconds              = 0
  max_message_size           = 262144  # 256 KB
  message_retention_seconds  = 345600  # 4 days
  receive_wait_time_seconds  = 10      # Long polling
  visibility_timeout_seconds = 30

  tags = {
    Name        = "${var.environment}-book-lending-queue"
    Environment = var.environment
    Application = "FictionBookLending"
  }
}

resource "aws_sqs_queue" "dead_letter_queue" {
  name                       = "${var.environment}-book-lending-dlq"
  delay_seconds              = 0
  max_message_size           = 262144
  message_retention_seconds  = 1209600  # 14 days
  receive_wait_time_seconds  = 0

  tags = {
    Name        = "${var.environment}-book-lending-dlq"
    Environment = var.environment
    Application = "FictionBookLending"
  }
}

# Redrive policy: Send failed messages to DLQ
resource "aws_sqs_queue_redrive_policy" "main_queue_redrive" {
  queue_url = aws_sqs_queue.main_queue.id

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.dead_letter_queue.arn
    maxReceiveCount     = 3
  })
}

# FIFO Queue for ordered messages
resource "aws_sqs_queue" "fifo_queue" {
  name                        = "${var.environment}-book-lending.fifo"
  fifo_queue                  = true
  content_based_deduplication = true
  deduplication_scope         = "messageGroup"
  fifo_throughput_limit       = "perMessageGroupId"

  tags = {
    Name        = "${var.environment}-book-lending-fifo"
    Environment = var.environment
    Application = "FictionBookLending"
  }
}

# -------------------------------
# Store SQS URLs in Parameter Store
# -------------------------------
resource "aws_ssm_parameter" "sqs_main_queue_url" {
  name        = "/${var.environment}/sqs/main-queue-url"
  description = "Main SQS Queue URL"
  type        = "String"
  value       = aws_sqs_queue.main_queue.url

  tags = {
    Environment = var.environment
  }
}

resource "aws_ssm_parameter" "sqs_dlq_url" {
  name        = "/${var.environment}/sqs/dlq-url"
  description = "Dead Letter Queue URL"
  type        = "String"
  value       = aws_sqs_queue.dead_letter_queue.url

  tags = {
    Environment = var.environment
  }
}

resource "aws_ssm_parameter" "sqs_fifo_queue_url" {
  name        = "/${var.environment}/sqs/fifo-queue-url"
  description = "FIFO Queue URL"
  type        = "String"
  value       = aws_sqs_queue.fifo_queue.url

  tags = {
    Environment = var.environment
  }
}
