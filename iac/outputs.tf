# -------------------------------
# Redis Outputs
# -------------------------------
output "redis_endpoint" {
  description = "Redis endpoint address"
  value       = aws_elasticache_cluster.fiction_book_redis.cache_nodes[0].address
}

output "redis_port" {
  description = "Redis port"
  value       = aws_elasticache_cluster.fiction_book_redis.port
}

output "redis_connection_string" {
  description = "Full Redis connection string"
  value       = "${aws_elasticache_cluster.fiction_book_redis.cache_nodes[0].address}:${aws_elasticache_cluster.fiction_book_redis.port}"
  sensitive   = true
}

output "ssm_parameter_name" {
  description = "SSM Parameter Store name for Redis connection"
  value       = aws_ssm_parameter.redis_connection_string.name
}

# -------------------------------
# SQS Outputs
# -------------------------------
output "sqs_main_queue_url" {
  description = "Main SQS Queue URL"
  value       = aws_sqs_queue.main_queue.url
}

output "sqs_main_queue_arn" {
  description = "Main SQS Queue ARN"
  value       = aws_sqs_queue.main_queue.arn
}

output "sqs_dead_letter_queue_url" {
  description = "Dead Letter Queue URL"
  value       = aws_sqs_queue.dead_letter_queue.url
}

output "sqs_fifo_queue_url" {
  description = "FIFO Queue URL"
  value       = aws_sqs_queue.fifo_queue.url
}

output "sqs_queue_names" {
  description = "All SQS Queue Names"
  value = {
    main_queue = aws_sqs_queue.main_queue.name
    dlq        = aws_sqs_queue.dead_letter_queue.name
    fifo_queue = aws_sqs_queue.fifo_queue.name
  }
}

output "ecr_repository_url" {
  description = "ECR repository URL"
  value       = aws_ecr_repository.demo_service.repository_url
}

output "ecs_cluster_name" {
  description = "ECS cluster name"
  value       = aws_ecs_cluster.ecs_cluster.name
}

output "ecs_service_name" {
  description = "ECS service name"
  value       = aws_ecs_service.ecs_service.name
}

output "ecs_task_definition_arn" {
  description = "Task definition ARN"
  value       = aws_ecs_task_definition.task_def.arn
}


